namespace Game.Util.Commands
{
    using Game.Robots;
    using Game.Robots.Breeding;
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
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
            public int Replicas { get; set; } = 1;

            [Option]
            public bool Firing { get; set; } = false;

            [Option]
            public string Name { get; set; } = null;

            [Option]
            public string Target { get; set; } = "";

            [Option]
            public string Color { get; set; } = null;

            [Option]
            public string Sprite { get; set; } = null;

            [Option("--type-name")]
            public string TypeName { get; set; } = null;

            [Option("--startup-delay")]
            public int StartupDelay { get; set; } = 0;

            [Option("--file")]
            public string File { get; set; } = null;

            [Option("--evolve")]
            public bool Evolve { get; set; } = false;

            protected async override Task ExecuteAsync()
            {
                if (StartupDelay > 0)
                    await Task.Delay(StartupDelay);

                ConfigurableContextBotConfig config = null;
                Type robotType = null;

                if (File != null)
                {
                    var fileName = Path.GetFullPath(File);
                    var text = System.IO.File.ReadAllText(fileName);
                    config = JsonConvert.DeserializeObject<ConfigurableContextBotConfig>(text);
                    if (config.RobotType != null)
                        robotType = Type.GetType(config.RobotType);
                }

                var tasks = new List<Task>();


                if (TypeName != null)
                    robotType = Type.GetType(TypeName);
                if (robotType == null)
                    if (config?.RobotType != null)
                        robotType = Type.GetType(config.RobotType);
                if (robotType == null)
                    robotType = typeof(ContextTurret);

                if (Name == null && config?.Name != null)
                    Name = config.Name;
                if (Name == null)
                    Name = "robot";

                if (Sprite == null && config?.Sprite != null)
                    Sprite = config.Sprite;
                if (Sprite == null)
                    Sprite = "ship_red";

                if (Color == null && config?.Color != null)
                    Color = config.Color;
                if (Color == null)
                    Color = "red";

                async Task<Robot> CreateRobot()
                {
                    var robot = Activator.CreateInstance(robotType) as Robot;
                    robot.AutoSpawn = true;
                    robot.AutoFire = Firing;
                    robot.Color = Color;
                    robot.Name = Name;
                    robot.Target = Target;
                    robot.Sprite = Sprite;
                    var connection = await API.Player.ConnectAsync(World);
                    robot.Connection = connection;

                    if (robot is ConfigurableContextBot configBot)
                        configBot.ConfigurationFileName = File;

                    return robot;
                }

                if (Evolve)
                {
                    var controller = new RobotEvolutionController();
                    var ga = controller.CreateGA(async (chromosome) =>
                    {
                        var robot = await CreateRobot() as ConfigurableContextBot;
                        if (robot == null)
                            throw new Exception("Failed to create robot or it isn't derived from ConfigurableContextBot");

                        return robot;
                    }, new RobotEvolutionConfiguration
                    {
                        BehaviorCount = 7,
                        FitnessDuration = 60000
                    });
                    ga.Start();
                }
                else
                {
                    for (int i = 0; i < Replicas; i++)
                    {
                        var robot = await CreateRobot();
                        tasks.Add(robot.StartAsync());
                    };

                    await Task.WhenAll(tasks);

                    foreach (var task in tasks)
                    {
                        if (task.IsFaulted)
                            Console.WriteLine($"Robot Crashed: {task.Exception}");
                    }
                }
            }
        }
    }
}