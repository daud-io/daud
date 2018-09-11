namespace Game.Engine.Core
{
    using System.Collections.Generic;
    using System;

    public class Player : IActor
    {
        public World World = null;
        public Fleet Fleet = null;

        public static Dictionary<World, List<Player>> Players = new Dictionary<World, List<Player>>();

        public int Score { get; set; }

        public ControlInput ControlInput { get; set; }
        private bool IsControlNew = false;

        public List<string> Messages { get; set; } = new List<string>();

        public bool IsAlive { get; set; } = false;

        public bool IsInvulnerable { get; set; } = false;

        public long spawnTime;

        public const int invulnTime = 5000;

        public string ShipSprite { get; set; }

        public void SetControl(ControlInput input)
        {
            this.ControlInput = input;
            this.IsControlNew = true;
        }

        public void Deinit()
        {
            Die();
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

            IsInvulnerable = true;
            spawnTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public string Name { get; set; }

        public static List<Player> GetWorldPlayers(World world)
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

        public virtual void Step()
        {
            if (!IsAlive)
                return;

            if (this.IsControlNew)
            {
                Fleet.Angle = ControlInput.Angle;
                Fleet.AimTarget = ControlInput.Position;
                Fleet.BoostRequested = ControlInput.BoostRequested;
                Fleet.ShootRequested = ControlInput.ShootRequested;
            }

            this.IsControlNew = false;

            if(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > spawnTime + invulnTime) {
                IsInvulnerable = false;
            }
        }

        protected virtual Fleet CreateFleet(string name, string color)
        {
            return new Fleet
            {
                Owner = this,
                Position = World.RandomPosition(),
                Caption = name,
                Color = color
            };
        }

        public void Spawn(string name, string sprite, string color)
        {
            if (name != null
                && name.Length > 15)
                name = name.Substring(0, 15);

            this.Name = name;

            if (!IsAlive)
            {
                IsAlive = true;

                Fleet = CreateFleet(name, color);

                ShipSprite = sprite;

                Fleet.Init(World);
            }
        }
        
        public void Die()
        {
            if (IsAlive)
            {
                Score /= 2;

                Fleet.Deinit();
                Fleet = null;
                IsAlive = false;
            }
        }

        public void SendMessage(string message)
        {
            this.Messages.Add(message);
        }

        public List<string> GetMessages()
        {
            if (Messages.Count > 0)
            {
                var m = Messages;
                Messages = new List<string>();
                return m;
            }
            else
                return null;
        }
    }
}
