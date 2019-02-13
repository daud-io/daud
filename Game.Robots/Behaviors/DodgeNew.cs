namespace Game.Robots.Behaviors
{
    using Game.API.Client;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class DodgeNew: ContextBehavior
    {
        private List<Body> DangerousBullets;

        public List<Vector2> ConsideredPoints { get; set; } = null;

        private List<Vector2> Projections;

        public DodgeNew(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
            var teamMode = Robot.HookComputer.TeamMode;

            DangerousBullets = Robot.SensorBullets.VisibleBullets
                .Where(b => b.Group.Owner != Robot.FleetID)
                .Where(b => !teamMode || b.Group.Color != Robot.Color)
                .ToList();

            Projections = DangerousBullets.Select(b => b.ProjectNew(Robot.GameTime + LookAheadMS).Position).ToList();
            ConsideredPoints = new List<Vector2>();
        }

        protected override float ScoreAngle(float angle, Vector2 position)
        {
            float accumulator = 0f;

            var fleet = Robot.SensorFleets.MyFleet;

            if (fleet != null)
            {
                ConsideredPoints.Add(position);

                foreach (var danger in Projections)
                {
                    var distSq = Vector2.DistanceSquared(danger, position);
                    if (distSq < 100000){
                        foreach (var ship in fleet.Ships){
                            var d=Vector2.DistanceSquared(danger, position+ship.Position-fleet.Center);
                            accumulator -= 1 / d;
                        }
                    }
                    
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
