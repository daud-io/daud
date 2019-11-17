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
            if(this.Fleet.BoostCooldownStatus==1){
            if(this.Fleet.Ships.Count<6){
                var nearfish = World.Bodies
                .Where(b => (b.Group!=null && b.Group.GroupType == GroupTypes.Fish) || b.Sprite == Sprites.fish)
                .OrderBy(p => Vector2.Distance(p.Position, this.Fleet.FleetCenter))
                .FirstOrDefault();
                if (nearfish != null){
                    vel = nearfish.Position - this.Fleet.FleetCenter;
                }
            }
            else if (player != null)
            {
                vel = player.Fleet.FleetCenter+player.Fleet.FleetMomentum*2 - this.Fleet.FleetCenter;
                vel -= Vector2.Normalize(vel) * 800;
            }
            else
            {
                var angle = (float)((World.Time - SpawnTime) / 3000.0f) * MathF.PI * 2;
                vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle))*800 - this.Fleet.FleetCenter;
            }
            }
            this.ControlInput.ShootRequested = true;

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
                    vel += avoid * 40_000 / distance;
                    var side = IsLeft(bullet.Position, bullet.Position+bullet.Momentum, this.Fleet.FleetCenter)?-1:1;
                    vel += new Vector2(side*bullet.Momentum.Y, -side*bullet.Momentum.X)*400*distance;
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
        public  bool IsLeft(Vector2 a, Vector2 b, Vector2 c) {
            return ((b.X - a.X)*(c.Y - a.Y) - (b.Y - a.Y)*(c.X - a.X)) > 0;
        }

        public static Vector2 FiringIntercept(
            Hook hook,
            Vector2 fromPosition,
            Vector2 targetPosition,
            Vector2 targetMomentum,
            int fleetSize,
            out int timeToImpact
        )
        {
            var toTarget = targetPosition - fromPosition;

            var bulletSpeed = fleetSize * hook.ShotThrustM + hook.ShotThrustB * 10;

            var a = Vector2.Dot(targetMomentum, targetMomentum) - (bulletSpeed * bulletSpeed);
            var b = 2 * Vector2.Dot(targetMomentum, toTarget);
            var c = Vector2.Dot(toTarget, toTarget);

            var p = -b / (2 * a);
            var q = MathF.Sqrt((b * b) - 4 * a * c) / (2 * a);

            var t1 = p - q;
            var t2 = p + q;
            var t = 0f;

            if (t1 > t2 && t2 > 0)
                t = t2;
            else
                t = t1;

            var aimSpot = targetPosition + targetMomentum * t;

            var bulletPath = aimSpot - fromPosition;
            timeToImpact = (int)(bulletPath.Length() / bulletSpeed);//speed must be in units per second            

            if (timeToImpact > hook.BulletLife)
                timeToImpact = int.MaxValue;

            return aimSpot;
        }
    }
}