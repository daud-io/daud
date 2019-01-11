namespace Game.Robots.Behaviors
{
    using Game.API.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class Dodge: ContextBehavior
    {
        private readonly ContextRobot Robot;
        private IEnumerable<Body> DangerousBullets;

        public int LookAheadMS { get; set; } = 250;

        private IEnumerable<Vector2> Projections;

        public Dodge(ContextRobot robot)
        {
            this.Robot = robot;
        }

        protected override void PreSweep(ContextRing ring)
        {
            DangerousBullets = Robot.SensorBullets.VisibleBullets.Where(b => b.Group.Owner != Robot.FleetID);
            Projections = DangerousBullets.Select(b => b.ProjectNew(Robot.GameTime + LookAheadMS).Position).ToList();
        }

        protected override float ScoreAngle(float angle)
        {
            float accumulator = 0f;

            var fleet = Robot.SensorFleets.MyFleet;
            if (fleet != null)
            {
                var projectedCenter = fleet.Center +
                    new Vector2(MathF.Cos(angle), MathF.Sin(angle))
                    * fleet.Momentum.Length() * LookAheadMS;

                foreach (var danger in Projections)
                {
                    var distSq = Vector2.DistanceSquared(danger, projectedCenter);
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
