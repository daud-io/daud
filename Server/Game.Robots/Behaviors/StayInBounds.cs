namespace Game.Robots.Behaviors
{
    using System;
    using System.Numerics;

    public class StayInBounds : ContextBehavior
    {
        public StayInBounds(ContextRobot robot) : base(robot)
        {
            Normalize = false;
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            float accumulator = 0f;

            var fleet = Robot.SensorFleets.MyFleet;
            if (fleet != null)
            {
                var oobX = (MathF.Abs(position.X) - Robot.WorldSize);
                var oobY = (MathF.Abs(position.Y) - Robot.WorldSize);

                if (oobX > 0)
                    accumulator -= oobX;
                if (oobY > 0)
                    accumulator -= oobY;
            }

            return accumulator;
        }
    }
}
