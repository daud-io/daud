namespace Game.Robots
{
    using Game.Robots.Targeting;
    using System.Threading.Tasks;
    using System;
    using System.Numerics;
    using System.Linq;
    using Game.Robots.Boost;

    public class ConfigTurret : ConfigurableContextBot
    {
        private readonly FleetTargeting FleetTargeting;
        private readonly AbandonedTargeting AbandonedTargeting;
        private readonly FishTargeting FishTargeting;
        public bool AttackFleets { get; set; } = true;
        public bool AttackFish { get; set; } = true;
        public bool Safe { get; set; } = false;
        public bool AttackAbandoned { get; set; } = true;

        public float TargetingAverageError { get; set; } = 0;
        public int FiringDelayMS { get; set; } = 0;
        public float FiringDelayChance { get; set; } = 1f;
        private long DeferShootUntil = 0;
        private bool LastCanShoot = false;

        public int TimeOffset { get; set; } = 0;

        public int BoostThreshold { get => SizeBoost.BoostThreshold; set => SizeBoost.BoostThreshold = value; } // for backwards compatibility
        public SixMinuteAbs SizeBoost { get; set; }

        public DefensiveBoost DefensiveBoost { get; set; }
        

        public ConfigTurret()
        {
            FleetTargeting = new FleetTargeting(this) { IsSafeShot = this.IsSafeShot };
            AbandonedTargeting = new AbandonedTargeting(this) { IsSafeShot = this.IsSafeShot };
            FishTargeting = new FishTargeting(this) { IsSafeShot = this.IsSafeShot };

            SizeBoost = new SixMinuteAbs(this) { BoostThreshold = 16 };
            DefensiveBoost = new DefensiveBoost(this);
        }

        public override long GameTime => base.GameTime + TimeOffset;

        public override void ShootAt(Vector2 target)
        {
            var angle = MathF.Atan2(target.Y, target.X);
            var mag = target.Length();

            var r = new Random();
            angle += (float)r.NextDouble() * TargetingAverageError * 2;

            base.ShootAt(new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * mag);
        }

        public bool IsSafeShot(float angle)
        {
            var safeSet = Safe
                ? SensorFleets.Others
                : SensorFleets.Others.Where(f => SensorAllies.IsAlly(f));

            if (this.SensorFleets.MyFleet != null)
                if (safeSet
                    .Any(f => RoboMath.MightHit(
                        this.HookComputer,
                        this.SensorFleets.MyFleet,
                        f,
                        angle)))
                    return false;

            return true;
        }

        protected async override Task AliveAsync()
        {
            var random = new Random();

            await base.AliveAsync();

            // when shooting becomes available
            // lets see if we should delay
            if (!LastCanShoot && CanShoot)
                DeferShootUntil = GameTime + 
                    ((random.NextDouble() < FiringDelayChance)
                        ? FiringDelayMS
                        : 0);
            LastCanShoot = CanShoot;

            if (CanShoot && DeferShootUntil < GameTime)
            {
                var target = (AttackFleets ? FleetTargeting.ChooseTarget() : null)
                    ?? (AttackAbandoned ? AbandonedTargeting.ChooseTarget() : null)
                    ?? (AttackFish ? FishTargeting.ChooseTarget() : null);

                if (target != null)
                    ShootAt(target.Position);
            }


            this.SizeBoost.Behave();
            this.DefensiveBoost.Behave();
        }
    }
}
