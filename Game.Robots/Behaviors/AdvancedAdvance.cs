namespace Game.Robots.Behaviors
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class AdvancedAdvance : ContextBehavior
    {
        private bool Active = true;
        private long ActiveUntil = 0;

        public Vector2 TargetPoint = Vector2.Zero;
        public int ActiveRange { get; set; } = 1000;
        public float AdvanceThreshold { get; set; } = 0.5f;
        public float BoostThreshold { get; set; } = 0;
        public int BoostMinimum { get; set; } = 0;

        public AdvancedAdvance(ContextRobot robot) : base(robot)
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
                    {
                        var ratio = (float)(closestOpponent.Ships.Count) / fleet.Ships.Count;
                        if (ratio > 0 && ratio < AdvanceThreshold)
                        {
                            TargetPoint = closestOpponent.Center + closestOpponent.Momentum * LookAheadMS;

                            bool isClosing =
                                Vector2.Distance(fleet.Center + fleet.Momentum * LookAheadMS, TargetPoint)
                                - Vector2.Distance(fleet.Center, TargetPoint) > 100;

                            if (ratio < BoostThreshold 
                                && fleet.Ships.Count >= BoostMinimum
                                && (Robot.GameTime - Robot.LastShot > 500)
                                //&& isClosing
                            )
                            {
                                Console.WriteLine($"{Robot.GameTime - Robot.LastShot}");

                                Robot.Boost();
                                ActiveUntil = Robot.GameTime + 400;
                            }

                            Active = true;
                        }
                    }
                }
            }

            base.PreSweep(ring);
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            if (Active || ActiveUntil < Robot.GameTime)
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
