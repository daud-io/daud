namespace Game.Robots
{
    using Game.API.Client;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    public class Robot
    {
        public bool AutoSpawn { get; set; } = true;
        private DateTime LastSpawn = DateTime.MinValue;
        private readonly PlayerConnection Connection;
        private const int RESPAWN_FALLOFF = 1000;
        private readonly DateTime Born = DateTime.Now;

        public bool AutoFire { get; set; } = false;

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
            float angle = (float)(DateTime.Now.Subtract(Born).TotalMilliseconds / 1000.0f) * MathF.PI * 2;

            this.Connection.ControlAimTarget = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 100;
            this.Connection.ControlIsShooting = true;

            await this.Connection.SendControlInputAsync();
        }

        private async Task StepDeadAsync()
        {
            if (AutoSpawn)
            {
                if (DateTime.Now.Subtract(LastSpawn).TotalMilliseconds > RESPAWN_FALLOFF)
                {
                    await Connection.SpawnAsync("Robot", "ship0", "green");
                    LastSpawn = DateTime.Now;
                }
            }
        }
    }
}
