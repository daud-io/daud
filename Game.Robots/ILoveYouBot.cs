namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Targeting;
    using System.Threading.Tasks;

    public class ILoveYouBot : ContextRobot
    {
        private readonly FleetTargeting FleetTargeting;
        private readonly AbandonedTargeting AbandonedTargeting;
        private readonly FishTargeting FishTargeting;

        public ILoveYouBot()
        {
            FleetTargeting = new FleetTargeting(this);
            AbandonedTargeting = new AbandonedTargeting(this);
            FishTargeting = new FishTargeting(this);

            Behaviors.AddRange(new IBehaviors[] {

                new Efficiency(this) { BehaviorWeight = 0.3f, Cycle = 500 },

                new Dodge(this) { LookAheadMS = 250, BehaviorWeight = 1 },
                new Dodge(this) { LookAheadMS = 500, BehaviorWeight = 1 },
                new Dodge(this) { LookAheadMS = 1000, BehaviorWeight = 1 },

                new Separation(this) { LookAheadMS = 500, BehaviorWeight = 0.1f },
                new StayInBounds(this) { LookAheadMS = 200, BehaviorWeight = 0.3f },

                new NavigateToPoint(this) { BehaviorWeight = 0.03f },

                new StayCloseToTeam(this) {BehaviorWeight = 0.3f, Cycle = 1000},

                new Slippery(this) { BehaviorWeight = 0.5f }
            });

            Steps = 16;
        }

        protected async override Task AliveAsync()
        {
            if (CanShoot)
            {
                var target = FleetTargeting.ChooseTarget()
                    ?? AbandonedTargeting.ChooseTarget()
                    ?? FishTargeting.ChooseTarget();

                if (target != null)
                    ShootAt(target.Position);
            }

            if (CanBoost && (this.SensorFleets.MyFleet?.Ships.Count ?? 0) > 10)
                Boost();

            await base.AliveAsync();
        }
    }
}
