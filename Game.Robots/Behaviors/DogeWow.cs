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
             var teamMode = Robot.HookComputer.Hook.TeamMode;

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
                    .Where(p => !Robot.HookComputer.Hook.TeamMode || p.Fleet.Color != Robot.Color);
                    foreach (var flet in muchFleets){
                        // Projections.Append(RoboMath.ProjectClosest( Robot.HookComputer,flet.Fleet.Center,Robot.Position,LookAheadMS,flet.Fleet.Ships.Count()));
                       
                    foreach (var ship in flet.Fleet.Ships){
                        
                            PhantomProjections.Append(RoboMath.ProjectClosest( Robot.HookComputer,ship.Position,Robot.Position,LookAheadMS,flet.Fleet.Ships.Count()));
                        }
                    }
            ConsideredPoints = new List<Vector2>();
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            float accumulator = 0f;

            var fleet = Robot.SensorFleets.MyFleet;

            if (fleet != null)
            {
                ConsideredPoints.Add(position);
                foreach (var ship in fleet.Ships){
                    float farA=40000*40000;
                     float farB=40000*40000;
                foreach (var danger in Projections)
                {
                    var distSq = Vector2.Distance(danger, position+ship.Position-fleet.Center);
                    if (distSq < 400){
                            //var d=Vector2.DistanceSquared(danger, position+ship.Position-fleet.Center);
                            farA=MathF.Min(distSq,farA);
                           // accumulator -= ((float)400*400*400*400) / (d*d+0.001f)/fleet.Ships.Count;
                        
                    }
                    
                }
                foreach (var danger in PhantomProjections)
                {
                    var distSq = Vector2.Distance(danger, position+ship.Position-fleet.Center);
                    if (distSq < 400){
                        farB=MathF.Min(distSq,farB);
                        
                            //var d=Vector2.DistanceSquared(danger, position+ship.Position-fleet.Center);
                           // accumulator -=( (float)400*400 )/ (d+1.0f)/fleet.Ships.Count/10.0f;
                        }
                    }
                    
                var thr=20.0f;
                if (farA < thr){
                        
                        
                            var d=farA;
                            accumulator -=1.0f/d/fleet.Ships.Count;//( 400.0f*400.0f )/ (farA+1.0f)/fleet.Ships.Count;
                        }
                        if (farB < thr/2.0f){
                        
                        
                            var d=farB;
                            accumulator -=1.0f/d/fleet.Ships.Count/4.0f;//( 400.0f*400.0f )/ (farA+1.0f)/fleet.Ships.Count;
                        }
                    }
            
            }
            

            return accumulator*1000.0f;//-0.5f+1.0f/(1.0f+MathF.Exp(-accumulator));
        }

        protected override void PostSweep(ContextRing ring)
        {
            //ring.Normalize();
        }
    }
}