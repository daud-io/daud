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
            using (DockerClient client = new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock"))
                 .CreateClient())
            {

                var container = await client.Containers.InspectContainerAsync(Environment.MachineName);

                var config = container.Config;
                var oldImage = config.Image;
                config.Image = $"iodaud/daud:{tag}";
                config.WorkingDir = null;

                await status($"{gameConfiguration.PublicURL} pulling image {config.Image}");
                await client.Images.CreateImageAsync(new ImagesCreateParameters
                {
                    FromImage = "iodaud/daud",
                    Tag = tag
                }, null, new Progress<JSONMessage>());

                var createContainerParameters = new CreateContainerParameters(config);
                createContainerParameters.HostConfig = container.HostConfig;

                var response = await client.Containers.CreateContainerAsync(createContainerParameters);


                var newID = response.ID;
                var oldID = container.ID;

                // finally launch another instance without Hostconfig
                createContainerParameters.HostConfig = null;
                createContainerParameters.Env.Add("GAME_DOCKER_UPGRADE=1");
                createContainerParameters.Env.Add("GAME_DOCKER_UPGRADE_OLD=" + oldID);
                createContainerParameters.Env.Add("GAME_DOCKER_UPGRADE_NEW=" + newID);
                await client.Containers.CreateContainerAsync(createContainerParameters);

                /*
                await client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());

                await status($"{gameConfiguration.PublicURL} {oldImage}->{config.Image}");

                if ((response.Warnings?.Count ?? 0) == 0)
                    await client.Containers.RemoveContainerAsync(Environment.MachineName, new ContainerRemoveParameters
                    {
                        Force = true
                    });

                Program.Abort();*/


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
