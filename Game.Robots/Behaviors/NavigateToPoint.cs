namespace Game.Robots.Behaviors
{
    using System;
    using System.Numerics;

    public class NavigateToPoint : ContextBehavior
    {
        private readonly ContextRobot Robot;
        public Vector2 TargetPoint = Vector2.Zero;

        public NavigateToPoint(ContextRobot robot)
        {
            this.Robot = robot;
        }

        private float CalculateDifferenceBetweenAngles(float firstAngle, float secondAngle)
        {
            float difference = secondAngle - firstAngle;
            if (difference < -MathF.PI) difference += 2 * MathF.PI;
            if (difference > MathF.PI) difference -= 2 * MathF.PI;
            return difference;
        }

        protected override float ScoreAngle(float angle)
        {
            var vectorToPoint = Robot.VectorToAbsolutePoint(TargetPoint);
            var angleToPoint = MathF.Atan2(vectorToPoint.Y, vectorToPoint.X);
            var difference = CalculateDifferenceBetweenAngles(angle, angleToPoint);
            return -MathF.Abs(difference);
        }
    }
}
