namespace Game.Robots
{
    using Game.Robots.Targeting;
    using System.Threading.Tasks;
    using System;
    using System.Numerics;

    public class ConfigTreeTurret : ConfigurableTreeBot
    {
        private readonly FleetTargeting FleetTargeting;
        private readonly AbandonedTargeting AbandonedTargeting;
        private readonly FishTargeting FishTargeting;
        public bool AttackFleets { get; set; } = true;
        public bool AttackFish { get; set; } = true;
        public bool Safe { get; set; } = false;
        public bool AttackAbandoned { get; set; } = true;
        public int BoostThreshold { get; set; } = 16;

        public float TargetingAverageError { get; set; } = 0;
        public int FiringDelayMS { get; set; } = 0;
        private long DeferShootUntil = 0;
        private bool LastCanShoot = false;

        public int TimeOffset { get; set; } = 0;

        public ConfigTreeTurret()
        {
            FleetTargeting = new FleetTargeting(this);
            AbandonedTargeting = new AbandonedTargeting(this);
            FishTargeting = new FishTargeting(this);
        }

        public override long GameTime => base.GameTime + TimeOffset;

        public override void ShootAt(Vector2 target)
        {
            var angle = MathF.Atan2(target.Y, target.X);
            var canShoot = true;
            if (this.firstRow != null)
            {
                var ddd = (int)MathF.Floor(((angle + 360.0f) % 360.0f) / 360.0f * (firstRow.Count - 1.0f));
                if (ddd < 0)
                {
                    ddd = 0;
                }
                if (ddd < firstRow.Count - 1)
                {
                    (var d, var c) = firstRow[ddd].bestChildScorePath();

                    if (d[d.Count - 1].Fleet.Ships.Count < 1)
                    {
                        canShoot = false;
                    }
                }
            }
            var mag = target.Length();

            var r = new Random();
            angle += (float)r.NextDouble() * TargetingAverageError * 2;
            if (canShoot)
            {
                base.ShootAt(new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * mag);
            }
        }

        protected async override Task AliveAsync()
        {
            await base.AliveAsync();

            // when shooting becomes available
            // lets see if we should delay
            if (!LastCanShoot && CanShoot)
                DeferShootUntil = GameTime + FiringDelayMS;
            LastCanShoot = CanShoot;

            if (CanShoot && DeferShootUntil < GameTime)
            {
                var target = (AttackFleets ? FleetTargeting.ChooseTarget() : null)
                    ?? (AttackAbandoned ? AbandonedTargeting.ChooseTarget() : null)
                    ?? (AttackFish ? FishTargeting.ChooseTarget() : null);

                if (target != null)
                {
                    var shootAngle = MathF.Atan2(target.Position.Y - this.Position.Y, target.Position.X - this.Position.X);
                    bool dangerous = false;
                    if (!AttackFleets && this.SensorFleets.MyFleet != null)
                    {
                        var flets = FleetTargeting.PotentialTargetFleets();
                        foreach (var flet in flets)
                        {
                            if (RoboMath.MightHit(this.HookComputer, this.SensorFleets.MyFleet, flet, shootAngle))
                            {
                                dangerous = true;
                            }
                        }
                    }
                    if (!Safe || !dangerous)
                    {
                        ShootAt(target.Position);
                    }
                }
            }

            if (CanBoost && (this.SensorFleets.MyFleet?.Ships.Count ?? 0) > BoostThreshold)
                Boost();

        }
    }
}
