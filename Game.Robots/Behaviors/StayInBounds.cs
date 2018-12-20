namespace Game.Robots.Behaviors
{
    using System;
    using System.Numerics;

    public class StayInBounds : ContextBehavior
    {
        private readonly ContextRobot Robot;

        public StayInBounds(ContextRobot robot)
        {
            this.Robot = robot;
        }

        protected override float ScoreAngle(float angle)
        {
            float accumulator = 0f;

            var fleet = Robot.SensorFleets.MyFleet;
            if (fleet != null)
            {
                var projectedCenter = fleet.Center +
                    new Vector2(MathF.Cos(angle), MathF.Sin(angle))
                    * fleet.Momentum.Length();

                var oobX = (MathF.Abs(projectedCenter.X) - Robot.WorldSize);
                var oobY = (MathF.Abs(projectedCenter.Y) - Robot.WorldSize);

                if (oobX > 0)
                    accumulator -= 1;
                if (oobY > 0)
                    accumulator -= 1;
            }

            return accumulator;
        }
    }
}
