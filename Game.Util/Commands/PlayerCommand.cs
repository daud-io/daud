namespace Game.Util.Commands
{
    using Game.Robots;
    using McMaster.Extensions.CommandLineUtils;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Subcommand("robots", typeof(Robots))]
    class PlayerCommand : CommandBase
    {
        class Robots : CommandBase
        {
            [Option]
            public string World { get; set; } = null;

            [Option]
            public int Replicas { get; set; } = 10;

            [Option]
            public bool Firing { get; set; } = false;

            [Option]
            public string Name { get; set; } = null;

            [Option]
            public string Color { get; set; } = "green";

            [Option]
            public string Sprite { get; set; } = "ship0";

            protected async override Task ExecuteAsync()
            {
                var tasks = new List<Task>();

                for (int i = 0; i < Replicas; i++)
                {
                    var player = await API.Player.ConnectAsync(World);
                    var robot = new Robot(player)
                    {
                        AutoSpawn = true,
                        AutoFire = Firing,
                        Color = Color,
                        Name = Name,
                        Sprite = Sprite
                    };

                    tasks.Add(robot.Start());
                };

                await Task.WhenAll(tasks);
            }
        }
    }
}
