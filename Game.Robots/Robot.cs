namespace Game.Robots
{
    using Game.API.Client;
    using Game.API.Common;
    using Game.API.Common.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    public class Robot
    {
        public PlayerConnection Connection { get; set; }

        public string Target { get; set; } = "";
        public string Name { get; set; } = "Robot";
        public string Sprite { get; set; } = "ship_cyan";
        public string Color { get; set; } = "cyan";

        private bool IsAlive = false;
        public bool AutoSpawn { get; set; } = true;
        public int RespawnFalloffMS { get; set; } = 3000;

        protected long SpawnTime { get; private set; }
        protected long DeathTime { get; private set; }

        public bool AutoFire { get; set; } = false;

        public bool CanShoot { get => CooldownShoot == 1 && !Shooting; }
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

        private bool IsSpawning = false;

        public bool Shooting { get; private set; }
        public Vector2 ShootingAt { get; private set; }
        public long ShootUntil { get; set; }
        public long ShootAfter { get; set; }

        public int ShootingTime { get; set; } = 100;
        public int ShootingDelay { get; set; } = 0;

        public long BoostUntil { get; set; }

        public int StatsKills { get; set; }
        public int StatsDeaths { get; set; }

        public bool DuelingProtocol { get; set; } = false;

        public string CustomData { get => Connection.CustomData; set => Connection.CustomData = value; }

        public Leaderboard Leaderboard { get => Connection.Leaderboard; }

        public HookComputer HookComputer { get; private set; }

        public Robot()
        {
            this.HookComputer = new HookComputer();
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
            => StartAsync(this.Connection, cancellationToken);

        public Task StartAsync(string server, string room, CancellationToken cancellationToken = default)
            => StartAsync(new PlayerConnection(server, room), cancellationToken);

        public virtual async Task StartAsync(PlayerConnection connection, CancellationToken cancellationToken = default)
        {
            this.Connection = connection;

            if (!this.Connection.IsConnected)
                await this.Connection.ConnectAsync();

            this.Connection.OnView = OnView;
            this.Connection.OnLeaderboard = OnLeaderboard;
            await this.Connection.ListenAsync(cancellationToken);
        }

        private async Task OnLeaderboard()
        {
            await this.OnNewLeaderboardAsync();
        }

        protected async virtual Task OnKillAsync(Announcement announcement)
        {
            if (DuelingProtocol)
            {
                await Task.Delay(80); // let's offset the starts, buggy concurrent spawn position
                await this.Connection.SendExitAsync();
                await this.Connection.APIClient.World.ResetWorldAsync(this.Connection.WorldKey);
            }
        }

        protected virtual Task OnKilledAsync(Announcement announcement)
        {
            return Task.FromResult(0);
        }

        protected virtual async Task OnAnnouncementAsync(Announcement announcement)
        {

            if (announcement.ExtraData != null)
            {
                var extraData = JsonConvert.DeserializeAnonymousType(announcement.ExtraData, new
                {
                    ping = new
                    {
                        you = 0,
                        them = 0
                    },
                    combo = new
                    {
                        text = null as string,
                        score = 0
                    },
                    stats = new
                    {
                        kills = 0,
                        deaths = 0
                    }
                });

                if (extraData != null && extraData.stats != null)
                {
                    StatsKills = extraData.stats.kills;
                    StatsDeaths = extraData.stats.deaths;

                    Log($"Stats: k:{StatsKills} d:{StatsDeaths} k/d:{(float)StatsKills / StatsDeaths:0.000}");
                }
            }

            switch (announcement.Type)
            {
                case "kill":
                    await OnKillAsync(announcement);
                    break;
                case "killed":
                    await OnKilledAsync(announcement);
                    break;
                default:
                    //this.Log(JsonConvert.SerializeObject(announcement, Formatting.Indented));
                    break;
            }
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
                IsSpawning = false;
            }

            IsAlive = Connection.IsAlive;

            while (Connection.Announcements.Count > 0)
                await this.OnAnnouncementAsync(Connection.Announcements.Dequeue());

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
            //await AliveAsync();

            if (!this.CanBoost && this.Connection.ControlIsBoosting && GameTime > BoostUntil)
                this.Connection.ControlIsBoosting = false;

            if (!this.Connection.ControlIsShooting
                && ShootAfter <= GameTime
                && GameTime < ShootUntil)
                Connection.ControlIsShooting = true;

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

            if (AutoSpawn && !IsSpawning)
                if (DeathTime + RespawnFalloffMS < GameTime)
                {
                    await SpawnAsync();
                }
        }

        protected async Task Exit()
        {
            await Connection.SendExitAsync();
        }

        protected async Task SpawnAsync()
        {
            IsSpawning = true;
            await Connection.SpawnAsync(Name, Sprite, Color);
        }

        public virtual void ShootAt(Vector2 target)
        {
            if (CanShoot)
            {
                Shooting = true;
                ShootingAt = target;
                ShootUntil = GameTime + ShootingTime;
                ShootAfter = GameTime + ShootingDelay;
            }
        }

        public Vector2 VectorToAbsolutePoint(Vector2 absolutePoint)
        {
            return absolutePoint - this.Position;
        }

        protected virtual void Log(string message)
        {
            lock (typeof(Console))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"[{this.FleetID}\t{this.Name}]\t");
                Console.ResetColor();
                Console.WriteLine(message);
            }
        }
    }
}
