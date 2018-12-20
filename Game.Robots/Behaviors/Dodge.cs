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

        private IEnumerable<Vector2> Projections;

        public Dodge(ContextRobot robot)
        {
            this.Robot = robot;
        }

        protected override void PreSweep()
        {
            var LookAheadMS = 1000;

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
                    * fleet.Momentum.Length();

                foreach (var danger in Projections)
                    accumulator -= Vector2.DistanceSquared(danger, projectedCenter);
            }

            return accumulator;
        }
    }
}
