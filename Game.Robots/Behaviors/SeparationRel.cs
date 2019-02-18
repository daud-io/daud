namespace Game.Robots.Behaviors
{
    using System.Numerics;
    using System;

    public class SeparationRel : ContextBehavior
    {
        public int ActiveRange { get; set; } = 10000;

        public SeparationRel(ContextRobot robot) : base(robot)
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
                    var fdist = Vector2.Distance(other.Center + other.Momentum * LookAheadMS, position);
                    var dist = fdist + 0.0f;
                    if (dist < ActiveRange * 2.0f)
                    {
                        foreach (var ship in fleet.Ships)
                        {
                            foreach (var ship2 in other.Ships)
                            {
                                dist = MathF.Min(Vector2.Distance(ship2.Position + other.Momentum * LookAheadMS, position + ship.Position - fleet.Center), dist);
                            }
                        }
                        if (dist < ActiveRange)
                        {
                            accumulator -= Vector2.Dot(other.Center + other.Momentum * LookAheadMS - position, new Vector2(MathF.Cos(angle), MathF.Sin(angle))) / fdist / dist;
                        }
                    }
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
