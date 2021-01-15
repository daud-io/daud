namespace Game.Robots.Behaviors
{
    using System.Numerics;
    using System;

    public class Separation : ContextBehavior
    {
        public int ActiveRange { get; set; } = 10000;

        public Separation(ContextRobot robot) : base(robot)
        {
            Normalize = false;
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
                    var dist = other.Center + other.Momentum * LookAheadMS - position;
                    var dist2 = dist.LengthSquared();
                    if (dist2 < ActiveRange*ActiveRange)
                        accumulator -= Vector2.Dot(Vector2.Normalize(dist), new Vector2(MathF.Cos(angle), MathF.Sin(angle))) / dist2;
                }
            }

            return accumulator;
        }
    }
}
