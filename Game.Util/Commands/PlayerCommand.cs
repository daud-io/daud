namespace Game.Util.Commands
{
    using Game.API.Client;
    using Game.API.Common.Models;
    using Game.Robots;
    using Game.Robots.Breeding;
    using Game.Robots.Contests;
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
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

            [Option("--url")]
            public string Url { get; set; } = null;

            [Option("--evolve")]
            public bool Evolve { get; set; } = false;

            [Option("--evolve-challenge-config")]
            public string EvolveChallengeConfig { get; set; } = null;

            [Option("--evolve-hook")]
            public string EvolveHook { get; set; } = null;

            [Option("--scenario")]
            public string Scenario { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                if (StartupDelay > 0)
                    await Task.Delay(StartupDelay);

                ConfigurableContextBotConfig config = null;
                Type robotType = null;

                if (Url != null || File != null)
                {
                    string text = null;

                    if (File != null)
                        text = System.IO.File.ReadAllText(Path.GetFullPath(File));

                    if (Url != null)
                        using (var webClient = new WebClient())
                            text = await webClient.DownloadStringTaskAsync(Url);

                    config = JsonConvert.DeserializeObject<ConfigurableContextBotConfig>(text);
                    if (config.RobotType != null)
                        robotType = Type.GetType(config.RobotType);
                }

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

                async Task<Robot> CreateRobot(Type innerRobotType = null, string worldKey = null, APIClient apiClient = null)
                {
                    var robot = Activator.CreateInstance(innerRobotType ?? robotType) as Robot;
                    robot.AutoSpawn = true;

                    if (robot is ConfigurableContextBot configBot)
                    {
                        configBot.ConfigurationFileName = File;
                        configBot.ConfigurationFileUrl = Url;
                        configBot.InitializeConfiguration();
                    }
                    if (robot is ConfigurableTreeBot configtBot)
                        configtBot.ConfigurationFileName = File;


                    robot.AutoFire = Firing;
                    robot.Color = Color;
                    robot.Name = Name;
                    robot.Target = Target;
                    robot.Sprite = Sprite;

                    var connection = await (apiClient ?? API)
                        .Player.ConnectAsync(worldKey ?? World);

                    robot.Connection = connection;

                    return robot;
                }

                if (Scenario != null)
                {
                    var scenario = await UriTools.LoadAsync<ScenarioConfiguration>(Scenario);

                    var tasks = new List<Task>();

                    var contest = await ContestGame.CreateGameAsync(API, scenario.HookURL);
                    
                    foreach (var robotName in scenario.Robots.Keys)
                    {
                        var robot = await ConfigurableContextBot.Load(scenario.Robots[robotName]);
                        robot.Name = robotName;
                        robot.DuelingProtocol = true;
                        robot.Connection = await contest.API.Player.ConnectAsync(contest.WorldKey);
                        tasks.Add(robot.StartAsync());
                    }

                    await Task.WhenAll(tasks);
                }

                if (Evolve)
                {
                    /*
                    var controller = new RobotEvolutionController();
                    var ga = controller.CreateGA(async (chromosome) =>
                    {
                        var contest = await ContestGame.CreateGameAsync(API);

                        contest.Hook = Hook.Default;
                        if (EvolveHook != null)
                            JsonConvert.PopulateObject(
                                await System.IO.File.ReadAllTextAsync(EvolveHook),
                                contest.Hook
                            );

                        contest.TestRobot = await CreateRobot(
                            worldKey: contest.WorldKey,
                            apiClient: contest.API
                        ) as ConfigurableContextBot;
                        if (contest.TestRobot == null)
                            throw new Exception("Failed to create robot or it isn't derived from ConfigurableContextBot");

                        contest.ChallengeRobot = await CreateRobot(
                            worldKey: contest.WorldKey,
                            apiClient: contest.API,
                            innerRobotType: typeof(ConfigTurret)
                        ) as ConfigurableContextBot;
                        contest.ChallengeRobot.ConfigurationFileName = EvolveChallengeConfig;
                        if (contest.ChallengeRobot == null)
                            throw new Exception("Failed to create robot or it isn't derived from ConfigurableContextBot");

                        return contest;
                    }, new RobotEvolutionConfiguration
                    {
                        BehaviorCount = 9,
                        FitnessDuration = 60000 * 10
                    });
                    ga.Start();
                    */
                }
                else
                {
                    var tasks = new List<Task>();
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