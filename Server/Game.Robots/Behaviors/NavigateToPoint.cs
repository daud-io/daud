namespace Game.Robots.Behaviors
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class NavigateToPoint : ContextBehavior
    {
        public Vector2 TargetPoint = Vector2.Zero;

        public NavigateToPoint(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
            if (Robot.LeaderHuntMode)
                TargetPoint = Robot.Leaderboard.Entries.FirstOrDefault()?.Position ?? Vector2.Zero;

            base.PreSweep(ring);
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            var vectorToPoint = Robot.VectorToAbsolutePoint(TargetPoint);
            var angleToPoint = MathF.Atan2(vectorToPoint.Y, vectorToPoint.X);
            var difference = RoboMath.CalculateDifferenceBetweenAngles(angle, angleToPoint);
            return -MathF.Abs(difference);
        }
    }
}
