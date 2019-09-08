namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.API.Common.Models;
    using Game.API.Common.Models.Auditing;
    using Game.Engine.Auditing;
    using Game.Engine.Core.Weapons;
    using Game.Engine.Networking;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Numerics;

    public class Player : IActor
    {
        public World World = null;
        public Fleet Fleet = null;

        public string PlayerID { get; set; }

        public Connection Connection { get; set; }

        public static Dictionary<World, List<Player>> Players = new Dictionary<World, List<Player>>();

        public int Score { get; set; }
        public int KillStreak { get; set; } = 0;
        public int KillCount { get; set; } = 0;
        public int DeathCount { get; set; } = 0;

        public int MaxCombo { get; set; }
        public long LastKillTime { get; set; } = 0;
        public int ComboCounter { get; set; } = 0;

        public ControlInput ControlInput { get; set; }
        private bool IsControlNew = false;

        public List<PlayerMessage> Messages { get; set; } = new List<PlayerMessage>();

        public bool IsAlive { get; set; } = false;
        public bool IsStillPlaying { get; set; } = false;
        public long AliveSince { get; set; } = 0;
        public long DeadSince { get; set; } = 0;
        public long TimeDeath { get; set; } = 0;

        public bool IsInvulnerable { get; set; } = false;
        public bool Backgrounded { get; internal set; }
        public bool IsShielded { get; set; } = false;

        public long SpawnTime;
        public int SpawnInvulnerableTime => World.Hook.SpawnInvulnerabilityTime;
        public long InvulnerableUntil = 0;
        public bool DisableSpawnInvulnerability { get; set; } = false;

        public Sprites ShipSprite { get; set; }
        public string Color { get; set; }
        public string Token { get; set; }

        public bool AuthenticationStarted { get; set; }
        public List<string> Roles { get; set; } = null;
        public string LoginName { get; set; }

        public bool PendingDestruction { get; set; } = false;
        private bool IsSpawning = false;

        public string IP { get; set; } = null;

        private bool CummulativeBoostRequested = false;
        private bool CummulativeShootRequested = false;

        public Vector2? SpawnLocation { get; set; } = null;
        public Vector2? SpawnMomentum { get; set; } = null;

        private bool IsGearhead = false;

        public Player()
        {
            PlayerID = Guid.NewGuid().ToString().Replace("-", "");
        }

        public void SetControl(ControlInput input)
        {
            if (input.BoostRequested)
                CummulativeBoostRequested = true;
            if (input.ShootRequested)
                CummulativeShootRequested = true;

            this.ControlInput = input;
            this.IsControlNew = true;
        }

        public virtual void CreateDestroy()
        {
            if (IsSpawning && !IsAlive && Fleet == null)
            {
                IsSpawning = false;

                IsAlive = true;

                Fleet = CreateFleet(Color);

                Fleet.SpawnLocation = SpawnLocation;
                Fleet.Init(World);

                RemoteEventLog.SendEvent(new AuditEventSpawn
                {
                    Player = this.ToAuditModelPlayer()
                }, World);

                if (World.Hook.GearheadName != null && this.Name == World.Hook.GearheadName)
                {
                    Fleet.BaseWeapon = new FleetWeaponRobot();
                    IsGearhead = true;
                }

                if (SpawnMomentum != null)
                    foreach (var ship in Fleet.NewShips)
                        ship.Momentum = SpawnMomentum.Value;

                if (!DisableSpawnInvulnerability)
                    SetInvulnerability(SpawnInvulnerableTime, true);

                SpawnTime = World.Time;
            }

            if (PendingDestruction)
            {
                Destroy();
                PendingDestruction = false;
            }

            IsStillPlaying = !PendingDestruction &&
                DeadSince > World.Time - World.Hook.PlayerCountGracePeriodMS;
        }

        public void Destroy()
        {
            Die();

            if (Fleet != null)
            {
                Fleet.Destroy();
                Fleet = null;
            }

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
            lock (typeof(Player))
            {
                List<Player> worldPlayers = null;
                if (!Players.ContainsKey(world))
                {
                    worldPlayers = new List<Player>();
                    Players.Add(world, worldPlayers);
                }
                else
                    worldPlayers = Players[world];

                return worldPlayers;
            }
        }

        public void SetInvulnerability(int duration, bool isShield = false)
        {
            if (duration == 0)
            {
                InvulnerableUntil = 0;
                IsInvulnerable = false;
                isShield = false;
            }
            else
            {
                InvulnerableUntil = World.Time + duration;
                IsInvulnerable = true;
                IsShielded = isShield;

                if (isShield && Fleet != null)
                    foreach (var ship in Fleet.Ships)
                        ship.ShieldStrength = World.Hook.ShieldStrength;
            }
        }

        public virtual void Think()
        {
            if (!IsAlive)
                return;

            if (TimeDeath > 0 && TimeDeath < World.Time)
                this.PendingDestruction = true;

            if (IsGearhead && Fleet.Ships.Count < 15)
            {
                var r = new Random();
                if (r.NextDouble() < World.Hook.GearheadRegen)
                    Fleet.AddShip();
            }


            if (this.IsControlNew)
            {
                if (float.IsNaN(ControlInput.Position.X))
                    ControlInput.Position = new System.Numerics.Vector2(0, 0);

                Fleet.AimTarget = ControlInput.Position;

                Fleet.BoostRequested = CummulativeBoostRequested;
                Fleet.ShootRequested = CummulativeShootRequested;

                CummulativeBoostRequested = false;
                CummulativeShootRequested = false;

                Fleet.CustomData = ControlInput.CustomData;

                if (Fleet.CustomData != null)
                {
                    var parsed = JsonConvert.DeserializeAnonymousType(Fleet.CustomData, new { magic = null as string });
                    if (parsed?.magic != null)
                        JsonConvert.PopulateObject(parsed.magic, this);
                }
            }

            if (this.Backgrounded)
                Fleet.AimTarget = Vector2.Zero;

            this.IsControlNew = false;

            if (IsInvulnerable)
            {
                if (!this.Fleet.FiringWeapon && CummulativeShootRequested && this.Fleet.ShootCooldownStatus == 1)
                    IsInvulnerable = false;

                if (World.Time > InvulnerableUntil)
                    IsInvulnerable = false;

                if (!IsInvulnerable)
                {
                    IsShielded = false;

                    foreach (var ship in Fleet?.Ships)
                        ship.ShieldStrength = 0;
                }
            }
        }

        protected virtual Fleet CreateFleet(string color)
        {
            if (World.NewFleetGenerator != null)
                return World.NewFleetGenerator(this, color);
            else
                return new Fleet
                {
                    Owner = this,
                    Caption = this.Name,
                    Color = color
                };
        }

        public void Spawn(string name, Sprites sprite, string color, string token)
        {
            // sanitize the name
            if (name != null
                && name.Length > World.Hook.MaxNameLength)
                name = name.Substring(0, World.Hook.MaxNameLength);
                
            if (name == "") { name = "Unknown Squadron #0"; }

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

            if (!string.IsNullOrEmpty(this.Token) && !string.IsNullOrEmpty(player?.Token))
                RemoteEventLog.SendEvent(new OnDeath
                {
                    token = this.Token,
                    name = this.Name,
                    killedBy = player?.Token
                });
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

                if (Fleet != null)
                    Fleet.PendingDestruction = true;

                Fleet = null;
                IsAlive = false;
            }
        }

        public void SendMessage(string message, string type = "message", int pointsDelta = 0, object extraData = null)
        {
            this.Messages.Add(new PlayerMessage
            {
                Type = type,
                Message = message,
                ExtraData = extraData,
                PointsDelta = pointsDelta
            });
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
                Momentum = this.Fleet?.FleetMomentum
            };
        }
    }
}
