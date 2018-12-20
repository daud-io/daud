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

        protected override float ScoreAngle(float angle)
        {
            var vectorToPoint = Robot.VectorToAbsolutePoint(TargetPoint);
            var angleToPoint = MathF.Atan2(vectorToPoint.Y, vectorToPoint.X);
            var difference = RoboMath.CalculateDifferenceBetweenAngles(angle, angleToPoint);
            return -MathF.Abs(difference);
        }
    }
}
