namespace Game.Robots.Behaviors
{
    using Game.Robots.Models;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public class Slippery : ContextBehavior
    {
        private Dictionary<Fleet, float> FiringInterceptAngles = null;
        //public float ThresholdAngle { get; set; } = MathF.PI / 
        private List<Fleet> FleetsOfConcern = null;
        public int MaximumRange { get; set; } = 10000;

        public Slippery(ContextRobot robot) : base(robot)
        {
            Normalize = false;
        }

        protected override void PreSweep(ContextRing ring)
        {
            FiringInterceptAngles = new Dictionary<Fleet, float>();

            var myFleet = this.Robot.SensorFleets.MyFleet;
            if (myFleet == null)
                return;

            FleetsOfConcern = null;
            foreach (var fleet in this.Robot.SensorFleets.Others)
                if (!this.Robot.SensorTeam.IsSameTeam(fleet))
                {
                    if (Vector2.Distance(fleet.Center, myFleet.Center) < MaximumRange)
                    {
                        if (FleetsOfConcern == null)
                            FleetsOfConcern = new List<Fleet>();
                        FleetsOfConcern.Add(fleet);
                        var angle = CalculateIntercept(fleet, myFleet.Center, myFleet.Momentum);
                        FiringInterceptAngles.Add(fleet, angle);
                    }
                }

            Sleep(750);
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            float accumulator = 0;

            if (FleetsOfConcern != null)
                foreach (var fleet in FleetsOfConcern)
                {
                    var original = FiringInterceptAngles[fleet];
                    var newAngle = CalculateIntercept(fleet, position, momentum, LookAheadMS);

                    if (!float.IsNaN(original) && !float.IsNaN(newAngle))
                        accumulator += MathF.Abs(RoboMath.CalculateDifferenceBetweenAngles(newAngle, original));
                }

            return accumulator;
        }

        private float CalculateIntercept(Fleet fleet, Vector2 position, Vector2 momentum, int projectedIntoFutureMS = 0)
        {
            var myFleet = this.Robot.SensorFleets.MyFleet;

            var projectedFleetCenter = fleet.Center + fleet.Momentum * projectedIntoFutureMS;

            var interceptPoint = RoboMath.FiringIntercept(this.Robot.HookComputer,
                projectedFleetCenter, position, momentum, myFleet.Ships.Count);

            var toTarget = interceptPoint - fleet.Center;
            return MathF.Atan2(toTarget.Y, toTarget.X);
        }
    }
}
