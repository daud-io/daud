namespace Game.Engine.Hosting
{
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using System;
    using System.Threading.Tasks;

    public class DockerUpgrade
    {
        public static async Task UpgradeAsync(GameConfiguration gameConfiguration, string tag, Func<string, Task> status)
        {
            // for this to work, the container must be launched with 
            // a mount for the docker socket
            //   -v /var/run/docker.sock:/var/run/docker.sock

            using (DockerClient client = new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock"))
                 .CreateClient())
            {

                var container = await client.Containers.InspectContainerAsync(Environment.MachineName);

                var config = container.Config;
                var oldImage = config.Image;

                await status($"{gameConfiguration.PublicURL} pulling image");

                ImagesCreateParameters imagesCreateParameters = new ImagesCreateParameters();

                if (tag.IndexOf(':') == 0)
                {
                    imagesCreateParameters.FromImage = "iodaud/daud";
                    imagesCreateParameters.Tag = tag;
                }
                else
                {
                    var parts = tag.Split(':');
                    imagesCreateParameters.FromImage = parts[0];
                    imagesCreateParameters.Tag = parts[1];

                }

                await client.Images.CreateImageAsync(imagesCreateParameters, null, new Progress<JSONMessage>());

                config.Image = $"{imagesCreateParameters.FromImage}:{imagesCreateParameters.Tag}";
                config.WorkingDir = null;

                var createContainerParameters = new CreateContainerParameters(config)
                {
                    HostConfig = container.HostConfig
                };

                createContainerParameters.Hostname = null;
                var response = await client.Containers.CreateContainerAsync(createContainerParameters);

                await status($"{gameConfiguration.PublicURL} {oldImage}->{config.Image}");


                var newID = response.ID;
                var oldID = container.ID;

                // would be nice if we could just start and stop the containers here...
                // but there's a huge gotcha. this container is exposing the same ports
                // that the new one will. They cannot be running at the same time.
                // So we use a 3rd container with no hostconfig (portmapping)
                // to do the switcheroo
                createContainerParameters.HostConfig.PortBindings = null;

                createContainerParameters.Env.Add("GAME_DOCKER_UPGRADE=1");
                createContainerParameters.Env.Add("GAME_DOCKER_UPGRADE_OLD=" + oldID);
                createContainerParameters.Env.Add("GAME_DOCKER_UPGRADE_NEW=" + newID);
                createContainerParameters.HostConfig.AutoRemove = true;
                createContainerParameters.HostConfig.RestartPolicy = null;

                response = await client.Containers.CreateContainerAsync(createContainerParameters);

                await client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters { });
            }
        }

        public static async Task FinalSwitchAsync(GameConfiguration gameConfiguration, string oldID, string newID)
        {
            using (DockerClient client = new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock"))
                 .CreateClient())
            {
                try
                {
                    // stop old container
                    await client.Containers.StopContainerAsync(oldID, new ContainerStopParameters
                    {
                        WaitBeforeKillSeconds = 1
                    });
                    // start new contianer
                    await client.Containers.StartContainerAsync(newID, new ContainerStartParameters { });
                    // delete old container
                    await client.Containers.RemoveContainerAsync(oldID, new ContainerRemoveParameters
                    {
                        Force = true
                    });
                }
                catch (Exception)
                {
                    // uh-oh, try to restart the old container
                    await client.Containers.StartContainerAsync(oldID, new ContainerStartParameters { });
                }
            }
        }
    }
}
