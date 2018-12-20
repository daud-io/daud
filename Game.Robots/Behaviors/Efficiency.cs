namespace Game.Robots.Behaviors
{
    using System;

    public class Efficiency : ContextBehavior
    {
        private readonly ContextRobot Robot;

        // reset each PreSweep
        private float TargetAngle;
        private float Scale = 0f;

        public Efficiency(ContextRobot robot)
        {
            this.Robot = robot;
        }

        protected override void PreSweep()
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

        protected override float ScoreAngle(float angle)
        {
            var difference = RoboMath.CalculateDifferenceBetweenAngles(angle, TargetAngle);
            return -MathF.Abs(difference) * Scale;
        }
    }
}
