namespace Game.Robots.Targeting
{
    using System.Linq;
    using System.Numerics;

    public class AbandonedTargeting : TargetingBase
    {
        public AbandonedTargeting(ContextRobot robot) : base(robot)
        {
        }

        public override Target ChooseTarget()
        {
            return this.Robot.SensorAbandoned.AllVisibleAbandoned
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
                        Abandoned = f,
                        Distance = Vector2.Distance(this.Robot.Position, f.Position),
                        Target = new Target
                        {
                            Position = intercept
                        },
                        Time = interceptTime
                    };
                })
                .Where(p => IsInViewport(p.Abandoned.Position))
                .OrderBy(p => p.Time)
                .FirstOrDefault()
                ?.Target;
        }
    }
}
