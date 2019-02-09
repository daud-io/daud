namespace Game.Util.Commands
{
    using Game.Robots;
    using McMaster.Extensions.CommandLineUtils;
    using System;
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

            [Option("--type-name")]
            public string TypeName { get; set; } = "Game.Robots.ContextTurret";

            [Option("--startup-delay")]
            public int StartupDelay{ get; set; } = 0;

            protected async override Task ExecuteAsync()
            {
                if (StartupDelay > 0)
                    await Task.Delay(StartupDelay);

                var tasks = new List<Task>();

                var type = Type.GetType(TypeName);

                for (int i = 0; i < Replicas; i++)
                {
                    var connection = await API.Player.ConnectAsync(World);
                    var robot = Activator.CreateInstance(type) as Robot;
                    robot.AutoSpawn = true;
                    robot.AutoFire = Firing;
                    robot.Color = Color;
                    robot.Name = Name;
                    robot.Target = Target;
                    robot.Sprite = Sprite;
                    tasks.Add(robot.Start(connection));

                };

                await Task.WhenAll(tasks);

                foreach (var task in tasks)
                {
                    if(task.IsFaulted)
                        Console.WriteLine($"Robot Crashed: {task.Exception}");
                }
            }
        }
    }
}