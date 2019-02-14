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
                    var distSq = Vector2.DistanceSquared(danger, position);
                    if (distSq < 100000)
                        accumulator -= 1 / distSq;
                }
            }

            return accumulator;
        }

        protected override void PostSweep(ContextRing ring)
        {
            ring.Normalize();
        }
    }
}
