namespace Game.Robots.Behaviors
{
    using System.Numerics;

    public class SeparationSimple : ContextBehavior
    {
        public int ActiveRange { get; set; } = 10000;

        public SeparationSimple(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            float accumulator = 0f;

            var fleet = Robot.SensorFleets.MyFleet;
            if (fleet != null)
            {
                foreach (var other in Robot.SensorFleets.Others)
                {

                    var dist = Vector2.Distance(other.Center + other.Momentum * LookAheadMS, position + momentum * LookAheadMS);
                    if (dist < ActiveRange)
                        accumulator -= dist;
                }
            }

            return accumulator / ActiveRange;
        }

        protected override void PostSweep(ContextRing ring)
        {
        }
    }
}