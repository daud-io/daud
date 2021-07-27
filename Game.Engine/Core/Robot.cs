namespace Game.Engine.Core
{
    using System;
    using System.Linq;
    using System.Numerics;
    using Game.API.Common;

    public class Robot : Player
    {
        public bool AutoSpawn { get; set; } = true;

        public Fleet ExcludeFleet { get; set; } = null;

        private long SpawnTimeAfter = 0;
        public bool OneLifeOnly { get; set; } = false;
        public bool AttackRobots { get; set; } = false;
        public int ShipSize { get; set; } = 70;

        public Robot() : base()
        {
        }

        protected override Fleet CreateFleet(string color)
        {
            return new RobotFleet
            {
                Owner = this,
                Caption = this.Name,
                Color = color,
                ShipSize = ShipSize
            };
        }


        public override void CreateDestroy()
        {
            base.CreateDestroy();

            if (!IsAlive)
            {
                if (AutoSpawn && World.Time > SpawnTimeAfter)
                    this.Spawn(Name, ShipSprite, "green", "");
            }
        }

        protected override void OnDeath(Player player = null)
        {
            base.OnDeath(player);

            if (OneLifeOnly)
            {
                PendingDestruction = true;
            }
            else
            {
                var delay = (World.Hook.BotRespawnDelay * World.AdvertisedPlayerCount) + 1;
                delay = delay < World.Hook.BotMaxRespawnDelay ? delay : World.Hook.BotMaxRespawnDelay;
                SpawnTimeAfter = World.Time + delay;
            }
        }

        public override void Think()
        {
            if (!IsAlive)
                return;

            var player =
                GetWorldPlayers(World)
                    .Where(p => p.IsAlive)
                    .Where(p => ExcludeFleet == null || (p.Fleet != ExcludeFleet && (p as Robot)?.ExcludeFleet != ExcludeFleet))
                    .Where(p => p.Fleet?.Ships?.Any() ?? false)
                    .Where(p => AttackRobots || (!p.Name?.StartsWith("🤖") ?? true))
                    .Where(p => p != this)
                    .OrderBy(p => Vector2.Distance(p.Fleet.FleetCenter, this.Fleet.FleetCenter))
                    .FirstOrDefault();

            var vel = Vector2.Zero;
            if (player != null)
            {
                vel += player.Fleet.FleetCenter - this.Fleet.FleetCenter;

                this.ControlInput.ShootRequested = true;
            }
            else
            {
                var angle = (float)((World.Time - SpawnTime) / 3000.0f) * MathF.PI * 2;
                vel += new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }
            var danger = false;
            var bullets = World.Bodies
                .Where(b => b.Group?.GroupType == GroupTypes.VolleyBullet || b.Group?.GroupType == GroupTypes.VolleySeeker)
                .Where(b => b.Group?.OwnerID != this.Fleet.ID)
                .OrderBy(p => Vector2.Distance(p.Position, this.Fleet.FleetCenter))
                .ToList();
            if (bullets.Any())
            {

                var bullet = bullets.First();

                var distance = Vector2.Distance(bullet.Position, this.Fleet.FleetCenter);
                if (distance < 2000)
                {
                    var avoid = (this.Fleet.FleetCenter - bullet.Position);
                    vel += avoid * 400_000 / avoid.LengthSquared();
                }
                if (distance < 200)
                {
                    danger = true;
                }
            }

            this.ControlInput.Position = vel;
            this.ControlInput.BoostRequested = danger;

            this.SetControl(ControlInput);

            base.Think();
        }
    }
}
