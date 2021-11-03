namespace Game.Robots.Herding
{
    using Game.API.Client;
    using Game.API.Common.Models;
    using Game.Robots.Monsters;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    public class Shepherd
    {
        public APIClient API { get; }
        public readonly string WorldKey;

        private readonly Timer TendTimer;
        private List<ConfigMonster> Herd = new List<ConfigMonster>();

        public string DefaultConfig { get; set; }

        public List<Vector2> SpawnLocations = new List<Vector2>();
        public bool Running => Herd.Count > 0;
        public string Announce { get; set; } = null;

        public Dictionary<string, string> RobotTypes { get; set; } = new Dictionary<string, string>();

        public string[][] Rows { get; set; } = new string[0][];
        public int RowSpeed { get; set; } = 4000;

        public string HookFile { get; set; } = null;

        public Shepherd(APIClient api, string defaultConfig, string worldKey = "robo")
        {
            this.API = api;
            this.WorldKey = worldKey;
            this.DefaultConfig = defaultConfig;

            this.TendTimer = new Timer(new TimerCallback((state) =>
            {
                Task.Run(this.AsyncTendEntry).Wait();

            }), this, TimeSpan.Zero, TimeSpan.FromMilliseconds(5000));

        }

        private async Task<ConfigMonster> StartRobotType(string type)
        {
            var configMonster = new ConfigMonster();
            configMonster.Configure(RobotTypes[type]);

            var baseMonsterType = Type.GetType(configMonster.RobotType);

            var robot = Activator.CreateInstance(baseMonsterType, this) as ConfigMonster;
            robot.Configure(RobotTypes[type]);
            robot.SensorAllies.AlliedNames.Add(robot.Name);
            await StartRobot(robot);

            return robot;
        }

        public async Task RunAsync()
        {

            if (Announce != null)
                await API.Server.AnnounceAsync(Announce, WorldKey);

            if (HookFile != null)
            {
                var hook = Hook.Default;
                JsonConvert.PopulateObject(File.ReadAllText(HookFile), hook);
                await API.World.PostHookAsync(hook, WorldKey);
            }

            int rowIndex = 0;
            while (rowIndex < Rows.Length)
            {

                var row = Rows[rowIndex];

                for (int spawnLocationIndex=0; spawnLocationIndex<row.Length; spawnLocationIndex++)
                {
                    var type = row[spawnLocationIndex];

                    if (type != null)
                    {
                        var robot = await StartRobotType(type);
                        
                        Console.WriteLine($"Spawning bot at : {SpawnLocations[spawnLocationIndex]}");
                        await robot.SpawnAtAsync(SpawnLocations[spawnLocationIndex]);
                    }
                }

                rowIndex++;

                await Task.Delay(RowSpeed);
            }

            while (this.Running)
                await Task.Delay(100);
        }

        private Task AsyncTendEntry()
        {
            Console.WriteLine($"Herd is {Herd.Count} strong");
            var disconnects = Herd.Where(r => !r.IsAlive && r.Lived && r.DestroyOnDeath).ToList();

            foreach (var disconnect in disconnects)
            {
                lock (Herd)
                    Herd.Remove(disconnect);

                try
                {
                    disconnect?.Connection?.Dispose();
                }
                catch(Exception) { }
            }

            /*if (Herd.Count(r => !r.IsAlive) > 0)
            {
                var ready = Herd.Where(r => !r.IsAlive).Take(SpawnLocationCount).ToList();
                for (int i = 0; i < ready.Count(); i++)
                {
                    Console.WriteLine($"Spawning bot at : {SpawnLocations[i]}");
                    await ready[i].SpawnAtAsync(SpawnLocations[i]);
                }
            }*/

            //if (Herd.Any() && !Herd.Any(r => r.IsAlive))
            //    await Herd[0].SpawnAsync();

            return Task.CompletedTask;
        }

        public Task OnSheepDeath(ConfigMonster sheep)
        {
            Console.WriteLine("one of the flock died of dysentery");
            return Task.CompletedTask;
        }

        public async Task<T> StartRobot<T>(T robot)
            where T : ConfigMonster
        {
            lock(Herd)
                Herd.Add(robot);

            robot.AutoSpawn = false;
            _ = robot.StartAsync(await API.Player.ConnectAsync(WorldKey));

            return robot;
        }

    }
}
