namespace Game.Robots.Behaviors
{
    using System.Numerics;

    public class SeparationClose: ContextBehavior
    {
        public SeparationClose(ContextRobot robot) : base(robot)
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
                    if(distSq<200.0f*200.0f){
                    accumulator -= 1 / distSq*1000.0f;
                    }
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
