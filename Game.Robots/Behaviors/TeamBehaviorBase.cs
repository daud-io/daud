namespace Game.Robots.Behaviors
{
    using Game.Robots.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public abstract class TeamBehaviorBase : ContextBehavior
    {
        protected List<Fleet> LocalTeammates = null;
        protected List<Vector2> RemoteTeamMates = null;
        protected bool Active = true;

        public TeamBehaviorBase(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
            LocalTeammates = new List<Fleet>();
            RemoteTeamMates = new List<Vector2>();

            if (this.Robot.HookComputer.Hook.TeamMode)
            {
                foreach (var entry in this.Robot.Leaderboard.Entries.Skip(2))
                {
                    if (this.Robot.SensorTeam.IsSameTeam(entry.Color)
                        && entry.FleetID != this.Robot.FleetID)
                    {
                        var fleet = this.Robot.SensorFleets.ByID(entry.FleetID);
                        if (fleet != null)
                            LocalTeammates.Add(fleet);
                        else
                            RemoteTeamMates.Add(entry.Position);
                    }
                }

                Active = true;

            }
            else
                Active = false;

            base.PreSweep(ring);
        }

        protected float ScoreAngleByTargetPoint(Vector2 target, float angle, Vector2 position, Vector2 momentum)
        {
            if (Active && target != null)
            {
                var vectorToPoint = Robot.VectorToAbsolutePoint(target);
                var angleToPoint = MathF.Atan2(vectorToPoint.Y, vectorToPoint.X);
                var difference = RoboMath.CalculateDifferenceBetweenAngles(angle, angleToPoint);
                return -MathF.Abs(difference);
            }

            return 0;
        }
    }
}
