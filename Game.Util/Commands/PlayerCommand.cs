namespace Game.Util.Commands
{
    using Game.Robots;
    using McMaster.Extensions.CommandLineUtils;
    using System.Collections.Generic;
    using System.Numerics;
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
            public string Name { get; set; } = "Robot";

            [Option]
            public string Target { get; set; } = "";

            [Option]
            public string Color { get; set; } = "green";

            [Option]
            public string Sprite { get; set; } = "ship0";

            [Option]
            public bool Variation { get; set; } = false;

            [Option]
            public bool DontFireAtSameName { get; set; } = false;

            [Option("--startup-delay")]
            public int StartupDelay{ get; set; } = 0;

            protected async override Task ExecuteAsync()
            {
                if (StartupDelay > 0)
                    await Task.Delay(StartupDelay);

                var tasks = new List<Task>();

                for (int i = 0; i < Replicas; i++)
                {

                    var connection = await API.Player.ConnectAsync(World);
                    var robot = new ContextTurret(Vector2.Zero)
                    {
                        AutoSpawn = true,
                        AutoFire = Firing,
                        Color = Color,
                        Name = Name,
                        Target = Target,
                        Sprite = Sprite,
                        DontFireAtSameName = DontFireAtSameName
                    };

                    if (Variation && i % 2 == 0)
                        robot.Vary();

                    tasks.Add(robot.Start(connection));
                };

                await Task.WhenAll(tasks);
            }
        }
    }
}
