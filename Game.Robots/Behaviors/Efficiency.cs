namespace Game.Robots.Behaviors
{
    using System;
    using System.Numerics;

    public class Efficiency : ContextBehavior
    {
        // reset each PreSweep
        private float TargetAngle;
        private float Scale = 0f;

        public Efficiency(ContextRobot robot): base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
            if (Robot.SensorFleets.MyFleet != null)
            {
                var momentum = Robot.SensorFleets.MyFleet.Momentum;
                TargetAngle = MathF.Atan2(momentum.Y, momentum.X);
                Scale = momentum.Length();
            }
            else
                Scale = 0;
        }

        protected override float ScoreAngle(float angle, Vector2 position)
        {
            var difference = RoboMath.CalculateDifferenceBetweenAngles(angle, TargetAngle);
            return -MathF.Abs(difference) * Scale;
        }
    }
}
