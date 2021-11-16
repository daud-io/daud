namespace Game.Robots.Targeting
{
    using Game.Robots.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class FleetTargeting : TargetingBase
    {
        public FleetTargeting(ContextRobot robot) : base(robot)
        {
        }

        public IEnumerable<Fleet> PotentialTargetFleets()
        {
            return Robot.SensorFleets.Others
                .Where(f => this.IsViableTarget(f));
        }

        protected bool IsViableTarget(Fleet fleet)
        {
            return
                (!Robot.SensorTeam.IsTeamMode || !Robot.SensorTeam.IsSameTeam(fleet))
                && !Robot.SensorAllies.IsAlly(fleet);
        }

        public override Target ChooseTarget()
        {
            return this.PotentialTargetFleets()
                .Select(f =>
                {
                    var intercept = RoboMath.FiringIntercept(
                        hook: this.Robot.HookComputer,
                        fromPosition: this.Robot.Position,
                        targetPosition: f.Center,
                        targetMomentum: f.Velocity,
                        fleetSize: this.Robot.SensorFleets.MyFleet?.Ships.Count ?? 0,
                        timeToImpact: out int interceptTime
                    );

                    return new
                    {
                        IsLeader = f.ID == Robot.Leaderboard.Entries.FirstOrDefault()?.FleetID,
                        Fleet = f,
                        Distance = Vector2.Distance(this.Robot.Position, f.Center),
                        Target = new Target
                        {
                            Position = intercept
                        },
                        Time = interceptTime
                    };
                })
                .Where(p => IsInViewport(p.Fleet.Center))
                .Where(p => IsSafeTarget(p.Target.Position) 
                    || (Robot.LeaderHuntMode && p.IsLeader))
                .OrderBy(p => p.Time)
                .FirstOrDefault()
                ?.Target;
        }
    }
}
