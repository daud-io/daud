namespace Game.Robots.Behaviors
{
    using Game.API.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class DogeWow : ContextBehavior
    {
        private IEnumerable<Body> DangerousBullets;

        public List<Vector2> ConsideredPoints { get; set; } = null;
        public int DistanceFromCenterThreshold { get; set; } = 316;

        private List<Vector2> Projections;
        private List<Vector2> PhantomProjections;
        public Vector2 ViewportCrop { get; set; } = new Vector2(2000 * 16f / 9f, 2000);

        public DogeWow(ContextRobot robot) : base(robot)
        {
            Normalize = false;
        }

        protected override void PreSweep(ContextRing ring)
        {
            var teamMode = Robot.HookComputer.Hook.TeamMode;

            DangerousBullets = Robot.SensorBullets.VisibleBullets
                .Where(b => b.Group.Owner != Robot.FleetID)
                .Where(b => !teamMode || b.Group.Color != Robot.Color)
                .ToList();

            Projections = DangerousBullets.Select(b => b.ProjectNew(Robot.GameTime + LookAheadMS).Position).ToList();
            PhantomProjections = new List<Vector2>();
            var muchFleets = Robot.SensorFleets.Others
                    .Select(f => new { Fleet = f, Distance = Vector2.Distance(Robot.Position, f.Center) })
                    .Where(p => MathF.Abs(p.Fleet.Center.X - Robot.Position.X) <= ViewportCrop.X
                        && MathF.Abs(p.Fleet.Center.Y - Robot.Position.Y) <= ViewportCrop.Y)
                    .Where(p => !Robot.HookComputer.Hook.TeamMode || p.Fleet.Color != Robot.Color);
            foreach (var flet in muchFleets)
            {
                // Projections.Append(RoboMath.ProjectClosest( Robot.HookComputer,flet.Fleet.Center,Robot.Position,LookAheadMS,flet.Fleet.Ships.Count()));

                foreach (var ship in flet.Fleet.Ships)
                {

                    PhantomProjections.Append(RoboMath.ProjectClosest(Robot.HookComputer, ship.Position, Robot.Position, LookAheadMS, flet.Fleet.Ships.Count()));
                }
            }
            ConsideredPoints = new List<Vector2>();
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            float accumulator = 0f;

            var fleet = Robot.SensorFleets.MyFleet;
            var dead = 0;
            if (fleet != null)
            {
                ConsideredPoints.Add(position);
                foreach (var ship in fleet.Ships)
                {
                    var shipDead = 0;
                    float farA = 40000 * 40000;
                    float farB = 40000 * 40000;
                    float minA = 0.0f;
                    float hitd = 90.0f;
                    foreach (var danger in Projections)
                    {
                        var dist = Vector2.Distance(danger, position + ship.Position - fleet.Center);
                        if (dist < DistanceFromCenterThreshold)
                        {
                            var fm = -hitd * hitd / MathF.Max(dist * dist, hitd * hitd) / fleet.Ships.Count * (2.0f + Vector2.Dot(danger - (position + ship.Position - fleet.Center), new Vector2(MathF.Cos(angle), MathF.Sin(angle))) / dist);
                            farA = MathF.Min(dist, farA);
                            minA = MathF.Min(fm, minA);
                            if (dist < hitd)
                            {
                                shipDead = 1;
                            }
                        }

                    }
                    foreach (var danger in PhantomProjections)
                    {
                        var dist = Vector2.Distance(danger, position + ship.Position - fleet.Center);
                        if (dist < DistanceFromCenterThreshold)
                        {
                            farB = MathF.Min(dist, farB);
                        }
                    }

                    var thr = DistanceFromCenterThreshold;
                    // if (farB < thr && farA < thr)
                    // {


                    //     var dist = farB;
                    //     var accumulator1 = -10.0f * 10.0f / (MathF.Max(dist * dist, 10.0f * 10.0f)) / fleet.Ships.Count/2.0f;
                    //     dist = farA;
                    //     accumulator1 = MathF.Min(accumulator1,-10.0f * 10.0f / (MathF.Max(dist * dist, 10.0f * 10.0f)) / fleet.Ships.Count);
                    //     accumulator+=accumulator1;
                    //     // accumulator -= 1.0f / (MathF.Max(dist*dist,10.0f*10.0f)+0.001f)/fleet.Ships.Count/2.0f;

                    //     // accumulator -=1.0f/d/fleet.Ships.Count/4.0f;//( 400.0f*400.0f )/ (farA+1.0f)/fleet.Ships.Count;
                    // }else{
                    if (farA < thr)
                    {


                        var dist = farA;
                        accumulator += minA;
                    }
                    // if (farB < thr)
                    // {


                    //     var dist = farB;
                    //     accumulator -= 1.0f / (MathF.Max(dist * dist, 10.0f * 10.0f)) / fleet.Ships.Count/2.0f;
                    //     // accumulator -= 1.0f / (MathF.Max(dist*dist,10.0f*10.0f)+0.001f)/fleet.Ships.Count/2.0f;

                    //     // accumulator -=1.0f/d/fleet.Ships.Count/4.0f;//( 400.0f*400.0f )/ (farA+1.0f)/fleet.Ships.Count;
                    // }
                    // }
                    dead += shipDead;
                }

            }
            if (fleet.Ships.Count < dead * 2 + 1)
            {
                return accumulator * 1000.0f;
            }

            return accumulator * ((float)dead + 1.0f);//-0.5f+1.0f/(1.0f+MathF.Exp(accumulator));
        }
    }
}