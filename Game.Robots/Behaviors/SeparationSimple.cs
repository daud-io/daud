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
            int count = 0;
            var fleet = Robot.SensorFleets.MyFleet;

            if (fleet != null)
            {
                foreach (var other in Robot.SensorFleets.Others)
                {

                    var dist = Vector2.Distance(other.Center + other.Velocity * LookAheadMS, position + momentum * LookAheadMS);
                    if (dist < ActiveRange)
                    {
                        accumulator = (1 - (dist / ActiveRange));
                        count++;
                    }
                }

            }

            return -accumulator;
        }

        protected override void PostSweep(ContextRing ring)
        {
        }
    }
}