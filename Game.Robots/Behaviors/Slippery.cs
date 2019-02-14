namespace Game.Robots.Behaviors
{
    using Game.Robots.Models;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public class Slippery : ContextBehavior
    {
        private Dictionary<Fleet, float> FiringInterceptAngles = null;

        public Slippery(ContextRobot robot): base(robot)
        {

        }

        protected override void PreSweep(ContextRing ring)
        {
            FiringInterceptAngles = new Dictionary<Fleet, float>();

            var myFleet = this.Robot.SensorFleets.MyFleet;
            if (myFleet == null)
                return;

            foreach (var fleet in this.Robot.SensorFleets.Others)
                if (!this.Robot.SensorTeam.IsSameTeam(fleet))
                    FiringInterceptAngles.Add(fleet, 
                        CalculateIntercept(fleet, myFleet.Center, myFleet.Momentum));

            Sleep(750);
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            float accumulator = 0;

            foreach (var fleet in this.Robot.SensorFleets.Others)
                if (!this.Robot.SensorTeam.IsSameTeam(fleet))
                {
                    var original = FiringInterceptAngles[fleet];
                    var newAngle = CalculateIntercept(fleet, position, momentum);
                    accumulator += MathF.Abs(RoboMath.CalculateDifferenceBetweenAngles(newAngle, original));
                }

            return accumulator;
        }

        private float CalculateIntercept(Fleet fleet, Vector2 position, Vector2 momentum)
        {
            var myFleet = this.Robot.SensorFleets.MyFleet;

            var interceptPoint = RoboMath.FiringIntercept(this.Robot.HookComputer,
                fleet.Center, position, momentum, myFleet.Ships.Count);

            var toTarget = interceptPoint - fleet.Center;
            return MathF.Atan2(toTarget.Y, toTarget.X);
        }
    }
}
