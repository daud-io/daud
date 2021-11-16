namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.API.Common.Models;
    using Game.API.Common.Models.Auditing;
    using Game.Engine.Networking;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading;

    public class Player : IActor
    {
        public World World = null;
        public Fleet Fleet = null;

        public string PlayerID { get; set; }

        public Connection Connection { get; set; }

        public int Score { get; set; }
        public int KillStreak { get; set; } = 0;
        public int KillCount { get; set; } = 0;
        public int DeathCount { get; set; } = 0;

        public int MaxCombo { get; set; }
        public long LastKillTime { get; set; } = 0;
        public int ComboCounter { get; set; } = 0;

        public Vector2 Position { get; set; } = Vector2.Zero;

        public List<PlayerMessage> Messages { get; set; } = new List<PlayerMessage>();

        public bool IsAlive { get; set; } = false;
        public bool IsStillPlaying { get; set; } = false;
        public long AliveSince { get; set; } = 0;
        public long DeadSince { get; set; } = 0;
        public long TimeDeath { get; set; } = 0;

        public bool IsInvulnerable { get; set; } = false;
        public bool Backgrounded { get; internal set; }
        public long SpawnTime;
        public long InvulnerableUntil = 0;

        public Sprites ShipSprite { get; set; }
        public string Color { get; set; }
        public string Token { get; set; }

        public bool AuthenticationStarted { get; set; }
        public List<string> Roles { get; set; } = null;
        public string LoginName { get; set; }

        public string Avatar { get; set; }

        public bool PendingDestruction { get; set; } = false;
        private bool IsSpawning = false;

        public string IP { get; set; } = null;
        private bool CummulativeBoostRequested = false;

        private bool CummulativeShootRequested = false;
        private int ControlPackets = 0;

        public Vector2? SpawnLocation { get; set; } = null;
        public Vector2? SpawnVelocity { get; set; } = null;

        private string UserColor = null;

        public float Advance = 0f;

        public Player()
        {
            PlayerID = Guid.NewGuid().ToString().Replace("-", "");
        }

        public void SetControl(Vector2 position, bool boost, bool shoot)
        {
            lock(this)
            {
                var packetNumber = Interlocked.Increment(ref this.ControlPackets);
                if (packetNumber == 1)
                {
                    CummulativeBoostRequested = false;
                    CummulativeShootRequested = false;
                }

                if (boost)
                    CummulativeBoostRequested = true;

                // if we find a control packet that requests firing
                if (shoot)
                {
                    // and it's the first one, then set the aim
                    if (!CummulativeShootRequested)
                        this.Position = position;

                    CummulativeShootRequested = true;
                }

                // if we haven't started shooting, then update the aiming with the latest packet
                if (!CummulativeShootRequested)
                    this.Position = position;
            }
        }

        public virtual void Create()
        {
            if (IsSpawning && !IsAlive && Fleet == null)
            {
                IsSpawning = false;

                IsAlive = true;

                Fleet = World.NewFleetGenerator(this);

                Fleet.SpawnLocation = SpawnLocation;

                /*RemoteEventLog.SendEvent(new AuditEventSpawn
                {
                    Player = this.ToAuditModelPlayer()
                }, World);*/

                InvulnerableUntil = World.Time + World.Hook.ShieldTimeMS;
                IsInvulnerable = true;

                SpawnTime = World.Time;
            }

            IsStillPlaying = DeadSince > World.Time - World.Hook.PlayerCountGracePeriodMS;
        }

        public void Destroy()
        {
            Die();

            if (this.Connection != null)
                try
                {
                    ((IDisposable)this.Connection).Dispose();
                }
                catch (Exception) { }
            this.Connection = null;

            World.Actors.Remove(this);

            var worldPlayers = GetWorldPlayers(World);
            worldPlayers.Remove(this);
        }

        public void Init(World world)
        {
            World = world;
            world.Actors.Add(this);

            var worldPlayers = GetWorldPlayers(world);
            worldPlayers.Add(this);

        }

        internal void ControlCharacter()
        {
            try
            {
                lock(this)
                {
                    this.ControlPackets = 0;
                    if (this.IsAlive && this.Fleet != null)
                    {
                        if (float.IsNaN(Position.X) || float.IsNaN(Position.Y))
                            Position = Vector2.Zero;

                        Fleet.AimTarget = Position;

                        Fleet.BoostRequested = CummulativeBoostRequested;
                        Fleet.ShootRequested = CummulativeShootRequested;

                        if (this.Backgrounded)
                            Fleet.AimTarget = Vector2.Zero;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in ControlCharachter: " + e);
            }
        }

        public string Name { get; set; }
        public ulong LoginID { get; set; }

        public static List<Player> GetTeam(World world, string color)
        {
            return Player.GetWorldPlayers(world)
                .Where(p => p.IsAlive)
                .Where(p => p.Color == color)
                .ToList();
        }

        public static List<Player> GetWorldPlayers(World world)
        {
            return world.Players;
        }

        public void Cleanup()
        {
            if (PendingDestruction)
                Destroy();
        }

        public virtual void Think(float dt)
        {
            Create();

            if (!IsAlive)
                return;

            if (TimeDeath > 0 && TimeDeath < World.Time)
                this.PendingDestruction = true;

            if (IsInvulnerable)
            {
                if (this.Fleet?.WeaponFiredCount > 0 || World.Time > InvulnerableUntil)
                {
                    IsInvulnerable = false;

                    foreach (var ship in Fleet?.Ships)
                        ship.ShieldStrength = 0;
                }
            }
        }

        public void Spawn(string name, Sprites sprite, string color, string token, string userColor = null)
        {
            if (IsAlive || IsSpawning)
                return;

            if (!World.CanSpawn)
            {
                if (World.CanSpawnReason != null)
                    SendMessage(World.CanSpawnReason);
                return;
            }

            UserColor = userColor;

            // sanitize the name
            if (name != null
                && name.Length > World.Hook.MaxNameLength)
                name = name.Substring(0, World.Hook.MaxNameLength);

            CummulativeBoostRequested = false;
            CummulativeShootRequested = false;

            Name = name;

            ShipSprite = sprite;
            Color = color;
            Token = token;

            AliveSince = World.Time;

            IsSpawning = true;
        }

        public void OnAuthenticated()
        {
            if (this.Connection != null)
            {
                this.Connection.Events.Enqueue(new BroadcastEvent
                {
                    EventType = "authenticated",
                    Data = JsonConvert.SerializeObject(new
                    {
                        roles = this.Roles,
                        loginName = this.LoginName
                    })
                });
            }
        }

        protected virtual void OnDeath(Player player = null)
        {
            if (Connection != null && player?.Fleet != null)
                Connection.SpectatingFleet = player.Fleet;

            /*if (!string.IsNullOrEmpty(player?.Token))
                RemoteEventLog.SendEvent(new OnDeath
                {
                    token = this.Token,
                    name = this.Name,
                    killedBy = player?.Token
                });*/
        }

        public void Exit()
        {
            if (IsAlive)
            {
                DeadSince = World.Time;
                OnDeath();

                if (Fleet != null)
                {
                    Fleet.Abandon();

                    Fleet.Ships.Clear();
                    Fleet.PendingDestruction = true;
                }

                Fleet = null;
                IsAlive = false;
            }
        }

        public void Die(Player player = null)
        {
            if (IsAlive)
            {
                DeadSince = World.Time;
                OnDeath(player);
            }

            if (Fleet != null)
            {
                Fleet.PendingDestruction = true;
                Fleet = null;
            }
            IsAlive = false;
        }

        public void SendMessage(string message, string type = "message", int pointsDelta = 0, object extraData = null)
        {
            try
            {
                if (message != null && this.Messages != null)
                    this.Messages.Add(new PlayerMessage
                    {
                        Type = type,
                        Message = message,
                        ExtraData = extraData,
                        PointsDelta = pointsDelta
                    });
            }
            catch (Exception) {}
        }

        public List<PlayerMessage> GetMessages()
        {
            if (Messages.Count > 0)
            {
                var m = Messages;
                Messages = new List<PlayerMessage>();
                return m;
            }
            else
                return null;
        }

        public AuditModelPlayer ToAuditModelPlayer()
        {
            return new AuditModelPlayer
            {
                LoginID = this.LoginID,
                LoginName = this.LoginName,
                PlayerID = this.PlayerID,
                FleetID = this.Fleet?.ID ?? 0,
                FleetName = this.Name,
                FleetSize = this.Fleet?.Ships?.Count ?? 0,
                Score = this.Score,
                AliveSince = this.AliveSince,
                Latency = this.Connection?.Latency ?? 0,
                KillCount = this.KillCount,
                KillStreak = this.KillStreak,
                ComboCounter = this.ComboCounter,
                MaxCombo = this.MaxCombo,

                Position = this.Fleet?.FleetCenter,
                Velocity = this.Fleet?.FleetVelocity
            };
        }
    }
}
