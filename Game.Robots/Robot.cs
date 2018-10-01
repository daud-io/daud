namespace Game.Robots
{
    using Game.API.Client;
    using System;
    using System.Threading.Tasks;

    public class Robot
    {
        public bool AutoSpawn { get; set; } = true;
        private DateTime LastSpawn = DateTime.MinValue;
        private readonly PlayerConnection Connection;
        private const int RESPAWN_FALLOFF = 1000;

        public Robot(PlayerConnection connection)
        {
            this.Connection = connection;
            this.Connection.OnView = this.OnView;
        }

        public async Task Start()
        {
            await this.Connection.ListenAsync();
        }

        private async Task OnView()
        {
            if (!Connection.IsAlive)
                await StepDeadAsync();
            else
                await StepAliveAsync();
        }

        private async Task StepAliveAsync()
        {
            Console.WriteLine("Hooray! I'm alive!");
        }

        private async Task StepDeadAsync()
        {
            if (AutoSpawn)
            {
                if (DateTime.Now.Subtract(LastSpawn).TotalMilliseconds < RESPAWN_FALLOFF)
                {
                    await Connection.SpawnAsync("Robot", "ship0", "green");
                    LastSpawn = DateTime.Now;
                }
            }
        }
    }
}
