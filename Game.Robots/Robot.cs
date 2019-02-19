namespace Game.Robots
{
    using Game.API.Client;
    using Game.Engine.Networking.Client;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Threading.Tasks;

    public class Robot
    {
        private Connection Connection;

        public string Target { get; set; } = "";
        public string Name { get; set; } = "Robot";
        public string Sprite { get; set; } = "ship_cyan";
        public string Color { get; set; } = "cyan";

        private bool IsAlive = false;
        public bool AutoSpawn { get; set; } = true;
        private const int RESPAWN_FALLOFF = 3000;

        protected long SpawnTime { get; private set; }
        protected long DeathTime { get; private set; }

        public bool AutoFire { get; set; } = false;

        public bool CanShoot { get => CooldownShoot == 1; }
        public bool CanBoost { get => CooldownBoost == 1; }

        public float CooldownShoot { get => Connection.CooldownShoot; }
        public float CooldownBoost { get => Connection.CooldownBoost; }

        public IEnumerable<Body> Bodies { get => Connection.Bodies; }
        public Vector2 Position { get => this.Connection.Position; }
        public virtual long GameTime { get => this.Connection.GameTime; }
        public ushort WorldSize { get => this.Connection.WorldSize; }
        public uint FleetID { get => this.Connection?.FleetID ?? 0; }

        protected virtual Task AliveAsync() => Task.FromResult(0);
        protected virtual Task DeadAsync() => Task.FromResult(0);
        protected virtual Task OnDeathAsync() => Task.FromResult(0);
        protected virtual Task OnSpawnAsync() => Task.FromResult(0);
        protected virtual Task OnNewLeaderboardAsync() => Task.FromResult(0);

        public bool Shooting { get; private set; }
        public Vector2 ShootingAt { get; private set; }
        public long ShootUntil { get; set; }
        public long ShootAfter { get; set; }

        public int ShootingTime { get; set; } = 100;
        public int ShootingDelay { get; set; } = 0;

        public long BoostUntil { get; set; }

        public string CustomData { get => Connection.CustomData; set => Connection.CustomData = value; }

        public Leaderboard Leaderboard { get => Connection.Leaderboard; }

        public HookComputer HookComputer { get; private set; }

        public Robot()
        {
            this.HookComputer = new HookComputer();
        }

        public Task StartAsync(string server, string room)
            => StartAsync(new Connection(server, room));

        public virtual async Task StartAsync(Connection connection)
        {
            this.Connection = connection;

            if (!this.Connection.IsConnected)
                await this.Connection.ConnectAsync();

            this.Connection.OnView = OnView;
            this.Connection.OnLeaderboard = OnLeaderboard;
            await this.Connection.ListenAsync();
        }

        private async Task OnLeaderboard()
        {
            await this.OnNewLeaderboardAsync();
        }

        protected virtual async Task OnView()
        {
            this.HookComputer.Hook = Connection.Hook;

            if (IsAlive && !Connection.IsAlive)
            {
                this.DeathTime = Connection.GameTime;
                await OnDeathAsync();
            }
            if (!IsAlive && Connection.IsAlive)
            {
                this.SpawnTime = Connection.GameTime;
                await OnSpawnAsync();
            }

            IsAlive = Connection.IsAlive;

            if (!Connection.IsAlive)
                await StepDeadAsync();
            else
                await StepAliveAsync();
        }

        protected void SteerAngle(float angle)
        {
            this.Connection.ControlAimTarget = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 800;
        }

        protected void SteerPointAbsolute(Vector2 point)
        {
            var relative = point - this.Position;
            this.Connection.ControlAimTarget = relative;
        }

        protected void SteerPointRelative(Vector2 point)
        {
            this.Connection.ControlAimTarget = point;
        }

        protected void SetSplit(bool splitting)
        {
            this.Connection.ControlIsBoosting = splitting;
        }

        private async Task StepAliveAsync()
        {
            await AliveAsync();

            if (!this.CanBoost && this.Connection.ControlIsBoosting && GameTime > BoostUntil)
                this.Connection.ControlIsBoosting = false;


            if (!this.CanShoot && this.Shooting && GameTime > ShootUntil)
            {
                this.Shooting = false;
                Connection.ControlIsShooting = false;
            }

            if (AutoFire)
            {
                this.Connection.ControlIsShooting = true;
                this.Shooting = true;
            }
            else
            {
                if (Shooting)
                    SteerPointAbsolute(ShootingAt);
            }

            await this.Connection.SendControlInputAsync();
        }

        public void Boost()
        {
            this.Connection.ControlIsBoosting = true;
            BoostUntil = GameTime + 100;
        }

        private async Task StepDeadAsync()
        {
            await DeadAsync();

            if (AutoSpawn)
                if (DeathTime + RESPAWN_FALLOFF < GameTime)
                {
                    await SpawnAsync();
                }
        }

        protected async Task SpawnAsync()
        {
            await Connection.SpawnAsync("🤖" + Name, Sprite, Color);
        }

        public virtual void ShootAt(Vector2 target)
        {
            if (CanShoot)
            {
                Shooting = true;
                ShootingAt = target;
                ShootUntil = GameTime + ShootingTime;
                ShootAfter = GameTime + ShootingDelay;
                Connection.ControlIsShooting = true;
            }
        }

        public Vector2 VectorToAbsolutePoint(Vector2 absolutePoint)
        {
            return absolutePoint - this.Position;
        }
    }
}
