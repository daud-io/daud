namespace Game.Robots.Behaviors
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class Advance : ContextBehavior
    {
        private bool Active = true;
        public Vector2 TargetPoint = Vector2.Zero;
        public int ActiveRange { get; set; } = 1000;
        public float AdvanceThreshold { get; set; } = 0.5f;

        public Advance(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
            Active = false;
            var fleet = Robot.SensorFleets.MyFleet;
            if (fleet != null)
            {
                if (Robot.SensorFleets.Others.Any())
                {
                    var closestOpponent = Robot.SensorFleets.Others
                        .Where(f => Vector2.Distance(f.Center, fleet.Center) < ActiveRange)
                        .OrderBy(f => Vector2.DistanceSquared(f.Center, fleet.Center))
                        .FirstOrDefault();
                    if (((closestOpponent?.Ships?.Count ?? 0) != 0) && ((fleet.Ships?.Count ?? 0) != 0))
                        if (closestOpponent.Ships.Count / fleet.Ships.Count < AdvanceThreshold)
                        {
                            TargetPoint = closestOpponent.Center;
                            Active = true;
                        }
                }
            }

            base.PreSweep(ring);
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            if (Active)
            {
                var vectorToPoint = Robot.VectorToAbsolutePoint(TargetPoint);
                var angleToPoint = MathF.Atan2(vectorToPoint.Y, vectorToPoint.X);
                var difference = RoboMath.CalculateDifferenceBetweenAngles(angle, angleToPoint);
                return -MathF.Abs(difference);
            }
            else
                return 0;
        }
    }
}
