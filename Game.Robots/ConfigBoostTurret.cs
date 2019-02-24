namespace Game.Robots
{
    using Game.Robots.Targeting;
    using System.Threading.Tasks;
    using Game.Robots.Behaviors;
    using Game.Robots.Models;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;


    public class ConfigBoostTurret : ConfigurableContextBot
    {
        private readonly FleetTargeting FleetTargeting;
        private readonly AbandonedTargeting AbandonedTargeting;
        private readonly FishTargeting FishTargeting;
        public bool AttackFleets = true;
        public bool AttackFish = true;
        public bool Safe = false;
        public bool AttackAbandoned = true;
        public int BoostThreshold { get; set; } = 2;

        public ConfigBoostTurret()
        {
            FleetTargeting = new FleetTargeting(this);
            AbandonedTargeting = new AbandonedTargeting(this);
            FishTargeting = new FishTargeting(this);
        }

        protected override Task AliveAsync()
        {
            // await base.AliveAsync();

            foreach (var sensor in Sensors)
                sensor.Sense();
            var contexts = ContextBehaviors.Select(b => b.Behave(Steps)).ToList();

            (var finalRing, var angle, var boost) = ContextRingBlending.Blend(contexts, true);
            SteerAngle(angle);
            var willShoot = CanShoot;
            if (CanBoost && boost && (this.SensorFleets.MyFleet?.Ships.Count ?? 0) > BoostThreshold)
            {
                if (ShootUntil < GameTime)
                {
                    Boost();
                }
                willShoot = false;
            }
            if (CanShoot && willShoot)
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



            if (ReloadConfigAfter > 0 && ReloadConfigAfter < GameTime)
            {
                LoadConfig();
                ReloadConfigAfter = 0;
            }

            return Task.FromResult(0);
        }
    }
}
