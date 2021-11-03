namespace Game.Robots.Behaviors
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class TeamAlignment : TeamBehaviorBase
    {
        private float? AverageAngle = 0f;

        public int MaximumRange { get; set; } = int.MaxValue;
        public int MinimumRange { get; set; } = 0;

        public TeamAlignment(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
            base.PreSweep(ring);

            AverageAngle = null;
            if (LocalTeammates.Any())
            {
                int count = 0;
                var averageMomentum = Vector2.Zero;

                foreach (var fleet in LocalTeammates)
                {
                    var distance = Vector2.Distance(fleet.Center, this.Robot.Position);

                    if (distance >= MinimumRange && distance <= MaximumRange)
                    {
                        averageMomentum += fleet.Velocity;
                        count++;
                    }
                }
                if (count > 0)
                {
                    averageMomentum /= count;
                    AverageAngle = MathF.Atan2(averageMomentum.Y, averageMomentum.X);
                }
            }
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            if (Active && AverageAngle != null)
                return -Math.Abs(RoboMath.CalculateDifferenceBetweenAngles(AverageAngle.Value, angle));

            return 0;
        }
    }
}
