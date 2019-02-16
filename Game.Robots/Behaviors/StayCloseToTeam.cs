namespace Game.Robots.Behaviors
{
    using Game.Robots.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class StayCloseToTeam : ContextBehavior
    {
        public Vector2? TargetPoint = null;

        public int MaximumRange { get; set; } = int.MaxValue;
        private List<Fleet> LocalTeammates = null;
        private List<Vector2> RemoteTeamMates = null;
        private bool Active = true;
        private float AverageAngle = 0;

        public StayCloseToTeam(ContextRobot robot) : base(robot)
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

                TargetPoint = null;
                if (RemoteTeamMates.Any())
                {
                    Vector2 accumulator = Vector2.Zero;
                    foreach (var remote in RemoteTeamMates)
                    {
                        accumulator += remote;
                    }
                    if (accumulator != Vector2.Zero)
                        TargetPoint = accumulator / RemoteTeamMates.Count;
                }

                AverageAngle = 0;
                if (LocalTeammates.Any())
                {
                    foreach (var fleet in LocalTeammates)
                        AverageAngle += RoboMath.CalculateDifferenceBetweenAngles(fleet.Ships.Average(s => s.Angle), 0);
                    AverageAngle /= LocalTeammates.Count;
                }


                Active = true;
                
            }
            else
                Active = false;

            base.PreSweep(ring);
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            if (Active)
            {
                // these really need to be separate rings so that they can be independently
                // normalized
                var alignmentScore = 1 - (RoboMath.CalculateDifferenceBetweenAngles(AverageAngle, angle) / Math.PI);
                var cohesionScore = 0;

                foreach (var fleet in LocalTeammates)
                {

                }

                float remoteScore = 0;
                if (TargetPoint != null)
                {
                    var vectorToPoint = Robot.VectorToAbsolutePoint(TargetPoint.Value);
                    var angleToPoint = MathF.Atan2(vectorToPoint.Y, vectorToPoint.X);
                    var difference = RoboMath.CalculateDifferenceBetweenAngles(angle, angleToPoint);
                    remoteScore = - MathF.Abs(difference);
                }

                return remoteScore;
            }
            else
                return 0;
        }

        protected override void PostSweep(ContextRing ring)
        {
            ring.Normalize();
        }
    }
}
