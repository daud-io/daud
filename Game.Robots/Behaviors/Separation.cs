namespace Game.Robots.Behaviors
{
    using System.Numerics;

    public class Separation: ContextBehavior
    {
        public Separation(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
        }

        protected override float ScoreAngle(float angle, Vector2 position)
        {
            float accumulator = 0f;

            var fleet = Robot.SensorFleets.MyFleet;
            if (fleet != null)
            {
                foreach (var other in Robot.SensorFleets.Others)
                {
                    var distSq = Vector2.DistanceSquared(other.Center, position);
                    if (distSq < 100000)
                        accumulator -= 1 / distSq;
                }
            }

            return accumulator;
        }

        protected override void PostSweep(ContextRing ring)
        {
            ring.Normalize();
        }
    }
}
