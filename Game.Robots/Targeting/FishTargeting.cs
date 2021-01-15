namespace Game.Robots.Targeting
{
    using System.Linq;
    using System.Numerics;

    public class FishTargeting : TargetingBase
    {
        public FishTargeting(ContextRobot robot) : base(robot)
        {
        }

        public override Target ChooseTarget()
        {
            return this.Robot.SensorFish.AllVisibleFish
                .Select(f =>
                {
                    var intercept = RoboMath.FiringIntercept(
                        hook: this.Robot.HookComputer,
                        fromPosition: this.Robot.Position,
                        targetPosition: f.Position,
                        targetMomentum: f.Momentum,
                        fleetSize: this.Robot.SensorFleets.MyFleet?.Ships.Count ?? 0,
                        timeToImpact: out int interceptTime
                    );

                    return new
                    {
                        Fish = f,
                        Target = new Target
                        {
                            Position = intercept
                        },
                        Time = interceptTime
                    };
                })
                .Where(p => IsInViewport(p.Fish.Position))
                .Where(p => IsSafeTarget(p.Target.Position))
                .OrderBy(p => p.Time)
                .FirstOrDefault()
                ?.Target;
        }
    }
}
