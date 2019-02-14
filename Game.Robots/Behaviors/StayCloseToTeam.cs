namespace Game.Robots.Behaviors
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class StayCloseToTeam : ContextBehavior
    {
        public Vector2 TargetPoint = Vector2.Zero;
        private bool Active = true;

        public StayCloseToTeam(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
            Vector2 accumulator = Vector2.Zero;
            int count = 0;

            if (this.Robot.HookComputer.Hook.TeamMode)
            {
                foreach (var entry in this.Robot.Leaderboard.Entries.Skip(2))
                {
                    if (this.Robot.SensorTeam.IsSameTeam(entry.Color) && entry.FleetID != this.Robot.FleetID)
                    {
                        accumulator += entry.Position;
                        count++;
                    }
                }

                if (count > 0)
                {
                    TargetPoint = accumulator / count;
                    Active = true;
                }
                else
                    Active = false;
                
            }
            base.PreSweep(ring);
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            if (Active)
            {
                var vectorToPoint = Robot.VectorToAbsolutePoint(TargetPoint);
                var angleToPoint = MathF.Atan2(vectorToPoint.Y, vectorToPoint.X);
                var difference = RoboMath.CalculateDifferenceBetweenAngles(angle, angleToPoint);
                return -MathF.Abs(difference);
            }
            else
                return 0;
        }
    }
}
