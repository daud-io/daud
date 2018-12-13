namespace Game.Robots.Behaviors
{
    using System;

    public class NavigateToPoint : ContextBehavior
    {
        private readonly Robot Robot;

        public NavigateToPoint(Robot robot)
        {
            this.Robot = robot;
        }

        private float CalculateDifferenceBetweenAngles(float firstAngle, float secondAngle)
        {
            float difference = secondAngle - firstAngle;
            while (difference < -MathF.PI) difference += 2*MathF.PI;
            while (difference > MathF.PI) difference -= 2 * MathF.PI;
            return difference;
        }

        protected override float ScoreAngle(float angle)
        {

            return 0;
        }
    }
}
