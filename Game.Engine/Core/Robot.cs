namespace Game.Engine.Core
{
    using System;
    using System.Linq;
    using System.Numerics;
    using Game.API.Common;
    using Game.API.Common.Models;

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

            if (OneLifeOnly) { 
                PendingDestruction = true;
            } else { 
                var delay = (World.Hook.BotRespawnDelay * World.AdvertisedPlayerCount) + 1;
                delay = delay < World.Hook.BotMaxRespawnDelay ? delay : World.Hook.BotMaxRespawnDelay;
                SpawnTimeAfter = World.Time + delay;
            }
        }

        public override void Think()
        {
            if (!IsAlive)
                return;

            this.ControlInput.ShootRequested = World.Time > this.Fleet.ShootCooldownTime;

            var player =
                GetWorldPlayers(World)
                    .Where(p => p.IsAlive)
                    .Where(p => ExcludeFleet == null || (p.Fleet != ExcludeFleet && (p as Robot)?.ExcludeFleet != ExcludeFleet))
                    .Where(p => (p.Fleet?.Ships?.Count() ?? 0) > 0)
                    .Where(p => AttackRobots || (!p.Name?.StartsWith("🤖") ?? true))
                    .Where(p => p != this)
                    .OrderBy(p => Vector2.Distance(p.Fleet.FleetCenter, this.Fleet.FleetCenter))
                    .FirstOrDefault();
            var vel = Vector2.Zero;
            if(this.Fleet.Ships.Count<5){
                var nearfish = World.Bodies
                .Where(b => (b.Group!=null && b.Group.GroupType == GroupTypes.Fish) || b.Sprite == Sprites.fish || b.Sprite == Sprites.ship_gray)
                .OrderBy(p => Vector2.Distance(p.Position, this.Fleet.FleetCenter))
                .FirstOrDefault();
                if (nearfish != null){
                    vel = nearfish.Position - this.Fleet.FleetCenter;
                }
            }
            else if (player != null)
            {
                var fraction = (float)Math.Max(Math.Min((this.Fleet.ShootCooldownStatus>0.1?(1.0-this.Fleet.ShootCooldownStatus):(this.Fleet.ShootCooldownStatus))*10,1.0),0.0);
                var dist = player.Fleet.FleetCenter - this.Fleet.FleetCenter;
                if(dist.LengthSquared()>1500*1500) vel = dist * fraction;
                else vel = new Vector2(-dist.Y,dist.X) * fraction;
                vel += Intercept(this.Fleet.FleetCenter, player.Fleet.FleetCenter, player.Fleet.FleetMomentum, (this.Fleet.Ships.Count * this.Fleet.ShotThrustM + this.Fleet.ShotThrustB)*10) * 1000.0f * (1.0f-fraction);
            }
            else
            {
                var angle = (float)((World.Time - SpawnTime) / 3000.0f) * MathF.PI * 2;
                vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle))*800 - this.Fleet.FleetCenter;
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

                var distance2 = Vector2.DistanceSquared(bullet.Position, this.Fleet.FleetCenter);
                if (distance2 < 2000*2000)
                {
                    var avoid = (this.Fleet.FleetCenter - bullet.Position);
                    vel += avoid * 400_000 / avoid.LengthSquared();
                    var side = IsLeft(bullet.Position, bullet.Position+bullet.Momentum, this.Fleet.FleetCenter)?-1:1;
                    vel += new Vector2(side*bullet.Momentum.Y, -side*bullet.Momentum.X)*4_000_000/distance2;
                }
                if (distance2 < 200*200)
                {
                    danger = true;
                }
            }

            this.ControlInput.Position = vel;
            this.ControlInput.BoostRequested = danger;

            this.SetControl(ControlInput);

            base.Think();
        }
        public bool IsLeft(Vector2 a, Vector2 b, Vector2 c) {
            return ((b.X - a.X)*(c.Y - a.Y) - (b.Y - a.Y)*(c.X - a.X)) > 0;
        }

        public static Vector2 Intercept(Vector2 a, Vector2 b, Vector2 u, float v_mag)  {
            var ab = Vector2.Normalize(b - a);
            var ui = u - Vector2.Dot(u, ab) * ab;
            var vj_mag = (float) Math.Sqrt(Math.Max(v_mag * v_mag - ui.LengthSquared(), 0.0));
            return ab * vj_mag + ui;
        }

    }
}