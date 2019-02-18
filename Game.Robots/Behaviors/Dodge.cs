namespace Game.Robots.Behaviors
{
    using Game.API.Client;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class Dodge: ContextBehavior
    {
        private IEnumerable<Body> DangerousBullets;

        public List<Vector2> ConsideredPoints { get; set; } = null;
        public int DistanceFromCenterThreshold { get; set; } = 316;

        private IEnumerable<Vector2> Projections;

        public Dodge(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
            var teamMode = Robot.HookComputer.Hook.TeamMode;

            DangerousBullets = Robot.SensorBullets.VisibleBullets
                .Where(b => b.Group.Owner != Robot.FleetID)
                .Where(b => !teamMode || b.Group.Color != Robot.Color)
                .ToList();

            Projections = DangerousBullets.Select(b => b.ProjectNew(Robot.GameTime + LookAheadMS).Position).ToList();
            ConsideredPoints = new List<Vector2>();
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            float accumulator = 0f;

            var fleet = Robot.SensorFleets.MyFleet;

            if (fleet != null)
            {
                ConsideredPoints.Add(position);

                foreach (var danger in Projections)
                {
                    var dist = Vector2.Distance(danger, position);
                    if (dist < DistanceFromCenterThreshold)
                        accumulator -= 1 / (dist*dist);
                }
            }

            return accumulator;
        }
    }
}
