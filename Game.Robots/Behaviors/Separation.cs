namespace Game.Robots.Behaviors
{
    using System.Numerics;
    using System;

    public class Separation : ContextBehavior
    {
        public int ActiveRange { get; set; } = 10000;

        public Separation(ContextRobot robot) : base(robot)
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
                    var dist = Vector2.Distance(other.Center + other.Momentum * LookAheadMS, position);
                    if (dist < ActiveRange)
                        accumulator -= Vector2.Dot(other.Center + other.Momentum * LookAheadMS-position,new Vector2(MathF.Cos(angle),MathF.Sin(angle)))/dist/dist/dist;

                }
            }

            return accumulator;
        }

        protected override void PostSweep(ContextRing ring)
        {
            //ring.Normalize();
        }
    }
}
