namespace Game.Robots.Herding
{
    using Game.API.Client;
    using Game.Robots.Monsters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class Shepherd
    {
        public APIClient API { get; }
        public readonly string WorldKey;

        private readonly Timer TendTimer;
        private List<ConfigMonster> Herd = new List<ConfigMonster>();

        public string DefaultConfig { get; set; }

        public Shepherd(APIClient api, string defaultConfig, string worldKey = "robo")
        {
            this.API = api;
            this.WorldKey = worldKey;
            this.DefaultConfig = defaultConfig;

            this.TendTimer = new Timer(new TimerCallback((state) =>
            {
                Task.Run(this.AsyncTendEntry).Wait();

            }), this, TimeSpan.Zero, TimeSpan.FromMilliseconds(2000));

        }

        private async Task AsyncTendEntry()
        {
            Console.WriteLine($"Herd is {Herd.Count} strong");

            if (Herd.Any() && !Herd.Any(r => r.SensorFleets.MyFleet != null))
                await Herd[0].SpawnAsync();
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

            _ = robot.StartAsync(await API.Player.ConnectAsync(WorldKey));

            return robot;
        }
    }
}
