namespace Game.Robots
{
    using Game.API.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class Robot
    {
        private Connection Connection;

        public string Name { get; set; } = "Robot";
        public string Sprite { get; set; } = "ship0";
        public string Color { get; set; } = "ship0";

        public bool AutoSpawn { get; set; } = true;
        private const int RESPAWN_FALLOFF = 1000;
        private DateTime LastSpawn = DateTime.MinValue;

        protected long SpawnTime { get; private set; }
        protected long DeathTime { get; private set; }

        private bool IsAlive = false;

        public bool AutoFire { get; set; } = false;

        public async Task Start(Connection connection)
        {
            this.Connection = connection;
            this.Connection.OnView = OnView;
            await this.Connection.ListenAsync();
        }

        private async Task OnView()
        {
            if (IsAlive && !Connection.IsAlive)
            {
                this.SpawnTime = Connection.GameTime;
                await OnDeathAsync();
            }
            if (!IsAlive && Connection.IsAlive)
                await OnSpawnAsync();

            IsAlive = Connection.IsAlive;

            if (!Connection.IsAlive)
                await StepDeadAsync();
            else
                await StepAliveAsync();
        }

        protected virtual Task AliveAsync()
        {
            return Task.FromResult(0);
        }

        protected virtual Task DeadAsync()
        {
            return Task.FromResult(0);
        }

        protected Task OnDeathAsync()
        {
            return Task.FromResult(0);
        }

        protected Task OnSpawnAsync()
        {
            return Task.FromResult(0);
        }

        public IEnumerable<Body> Bodies
        {
            get
            {
                return Connection.Bodies;
            }
        }

        public Vector2 Position
        {
            get
            {
                return this.Connection.Position;
            }
        }

        public long GameTime
        {
            get
            {
                return this.Connection.GameTime;
            }
        }

        public uint FleetID
        {
            get
            {
                return this.Connection.FleetID;
            }
        }

        protected void SteerAngle(float angle)
        {
            var centerVector = -1 * this.Connection.Position;
            angle = MathF.Atan2(centerVector.Y, centerVector.X);
            this.Connection.ControlAimTarget = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 100;
            
        }

        protected void SteerPointAbsolute(Vector2 point)
        {
            var relative = this.Connection.Position - point;
            this.Connection.ControlAimTarget = relative;
        }

        protected void SteerPointRelative(Vector2 point)
        {
            this.Connection.ControlAimTarget = point;
        }

        private async Task StepAliveAsync()
        {
            await AliveAsync();

            if (AutoFire)
                this.Connection.ControlIsShooting = true;

            await this.Connection.SendControlInputAsync();
        }

        private async Task StepDeadAsync()
        {

            await DeadAsync();

            if (AutoSpawn)
            {
                if (DateTime.Now.Subtract(LastSpawn).TotalMilliseconds > RESPAWN_FALLOFF)
                {
                    await Connection.SpawnAsync(Name, Sprite, Color);
                    LastSpawn = DateTime.Now;
                }
            }
        }
    }
}
