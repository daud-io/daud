namespace Game.Engine.Core
{
    using System;
    using System.Linq;
    using System.Numerics;
    using Game.API.Common;

    public class Robot : Player
    {
        public bool AutoSpawn { get; set; } = true;

        private long SpawnTimeAfter = 0;

        public Robot() : base()
        {
        }

        protected override Fleet CreateFleet(string color)
        {
            return new RobotFleet
            {
                Owner = this,
                Caption = this.Name,
                Color = color
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

            SpawnTimeAfter = World.Time + World.Hook.BotRespawnDelay;
        }

        public override void Think()
        {
            if (!IsAlive)
                return;

            var player =
                GetWorldPlayers(World).OrderByDescending(p => p.Score)
                    .Where(p => p.IsAlive)
                    .Where(p => (p.Fleet?.Ships?.Count() ?? 0) > 0)
                    .Where(p => !p.Name?.StartsWith("🤖") ?? true)
                    .OrderBy(p => Vector2.Distance(p.Fleet.FleetCenter, this.Fleet.FleetCenter))
                    .FirstOrDefault();
            var vel = Vector2.Zero;
            if (player != null)
            {
                vel+=player.Fleet.FleetCenter - this.Fleet.FleetCenter;

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