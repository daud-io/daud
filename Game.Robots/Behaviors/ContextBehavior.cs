using System.Numerics;

namespace Game.Robots.Behaviors
{
    public class ContextBehavior : IBehaviors
    {
        public virtual float BehaviorWeight { get; set; } = 1f;
        protected readonly ContextRobot Robot;
        public int LookAheadMS { get; set; } = 100;

        public ContextBehavior(ContextRobot robot)
        {
            this.Robot = robot;
        }

        public ContextRing Behave(int steps)
        {
            var ring = new ContextRing(steps);
            this.PreSweep(ring);

            if (Robot?.SensorFleets?.MyFleet?.Ships != null)
            {
                for (var i = 0; i < steps; i++)
                {
                    var position = RoboMath.ShipThrustProjection(Robot.HookComputer,
                        Robot.Position,
                        Robot.SensorFleets.MyFleet.Momentum,
                        Robot.SensorFleets.MyFleet.Ships.Count,
                        ring.Angle(i),
                        LookAheadMS
                    );

                    ring.Weights[i] = ScoreAngle(ring.Angle(i), position);
                }
            }

            ring.RingWeight = BehaviorWeight;

            this.PostSweep(ring);

            ring.Name = this.GetType().Name;
            return ring;
        }

        public virtual void Reset()
        {
        }

        protected virtual void PreSweep(ContextRing ring)
        {
        }

        protected virtual void PostSweep(ContextRing ring)
        {
        }

        protected virtual float ScoreAngle(float angle, Vector2 position) => 0f;
    }
}
