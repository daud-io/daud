namespace Game.Robots.Behaviors
{
    using Game.API.Client;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using System.Numerics;

    public class DogeWow: ContextBehavior
    {
        private List<Body> DangerousBullets;
        

        public List<Vector2> ConsideredPoints { get; set; } = null;

        private List<Vector2> Projections;
        private List<Vector2> PhantomProjections;
        public Vector2 ViewportCrop { get; set; } = new Vector2(2000 * 16f / 9f, 2000);

        public DogeWow(ContextRobot robot) : base(robot)
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
            PhantomProjections= new List<Vector2>();
            var muchFleets=Robot.SensorFleets.Others
                    .Select(f => new { Fleet = f, Distance = Vector2.Distance(Robot.Position, f.Center) })
                    .Where(p => MathF.Abs(p.Fleet.Center.X - Robot.Position.X) <= ViewportCrop.X
                        && MathF.Abs(p.Fleet.Center.Y - Robot.Position.Y) <= ViewportCrop.Y)
                    .Where(p => !Robot.HookComputer.TeamMode || p.Fleet.Color != Robot.Color);
                    foreach (var flet in muchFleets){
                        Projections.Append(RoboMath.ProjectClosest( Robot.HookComputer,flet.Fleet.Center,Robot.Position,LookAheadMS,flet.Fleet.Ships.Count()));
                       
                    foreach (var ship in flet.Fleet.Ships){
                        
                            PhantomProjections.Append(RoboMath.ProjectClosest( Robot.HookComputer,ship.Position,Robot.Position,LookAheadMS,flet.Fleet.Ships.Count()));
                        }
                    }
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
                            accumulator -= 1 / (d+0.001f)/fleet.Ships.Count;
                        }
                    }
                    
                }
                foreach (var danger in PhantomProjections)
                {
                    var distSq = Vector2.DistanceSquared(danger, position);
                    if (distSq < 100000){
                        foreach (var ship in fleet.Ships){
                            var d=Vector2.DistanceSquared(danger, position+ship.Position-fleet.Center);
                            accumulator -= 1 / (d+0.001f)/fleet.Ships.Count/10.0f;
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
