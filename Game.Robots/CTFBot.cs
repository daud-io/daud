namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Models;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class CTFBot : ContextRobot
    {
        protected readonly NavigateToPoint Navigation;
        protected readonly Efficiency Efficiency;
        protected readonly Dodge Dodge0;
        protected readonly Dodge Dodge1;
        protected readonly Dodge Dodge2;
        protected readonly StayInBounds StayInBounds;
        protected readonly Separation Separation;

        public bool DontFireAtSameName { get; set; } = false;

        public Vector2 ViewportCrop { get; set; } = new Vector2(2000 * 16f / 9f, 2000);
        public int BoostThreshold { get; set; } = 16;

        public CTFBot(Vector2 target)
        {
            Behaviors.Add(Navigation = new NavigateToPoint(this) { BehaviorWeight = 0.00f });
            Behaviors.Add(Efficiency = new Efficiency(this) { BehaviorWeight = 0.1f });
            Behaviors.Add(Dodge0 = new Dodge(this) { LookAheadMS = 250, BehaviorWeight = 2 });
            Behaviors.Add(Dodge1 = new Dodge(this) { LookAheadMS = 500, BehaviorWeight = 2 });
            Behaviors.Add(Dodge2 = new Dodge(this) { LookAheadMS = 1000, BehaviorWeight = 2 });
            Behaviors.Add(Separation = new Separation(this) { LookAheadMS = 500, BehaviorWeight = 0f });
            Behaviors.Add(StayInBounds = new StayInBounds(this) { LookAheadMS = 1000, BehaviorWeight = 1f });

            Navigation.TargetPoint = target;
            Steps = 16;
        }

        public void Vary()
        {
            //Efficiency.BehaviorWeight = 0.5f;
            Color = "cyan";
            Sprite = "ship_cyan";

            Separation.BehaviorWeight = 1;

            Name += "++";
        }

        protected async override Task AliveAsync()
        {
            if (CanShoot)
            {
                var closest = SensorFleets.Others
                    .Select(f => new { Fleet = f, Distance = Vector2.Distance(this.Position, f.Center) })
                    .Where(p => MathF.Abs(p.Fleet.Center.X - this.Position.X) <= ViewportCrop.X
                        && MathF.Abs(p.Fleet.Center.Y - this.Position.Y) <= ViewportCrop.Y)
                    .Where(p => !DontFireAtSameName || p.Fleet.Name != this.Name)
                    .Where(p => p.Fleet.Name.Contains(this.Target) || this.Target == "")
                    .Where(p => !HookComputer.TeamMode || p.Fleet.Color != this.Color)
                    .OrderBy(p => p.Distance)
                    .FirstOrDefault()
                    ?.Fleet;

                if (closest != null)
                    ShootAtFleet(closest);
            }

            if (CanBoost && (SensorFleets.MyFleet?.Ships.Count ?? 0) > BoostThreshold)
                Boost();

            /*CustomData = JsonConvert.SerializeObject(new
            {
                spots = Behaviors.OfType<Dodge>().Where(d => d.ConsideredPoints != null).SelectMany(d => d.ConsideredPoints)
            });*/


            CustomData = JsonConvert.SerializeObject(new
            {
                spots = SensorFleets.Others?.Select(f => RoboMath.FiringIntercept(HookComputer, this.Position, f.Center, f.Momentum, this.SensorFleets.MyFleet?.Ships.Count ?? 0))
            });


            Console.WriteLine(JsonConvert.SerializeObject(Leaderboard, Formatting.Indented));

//            Console.WriteLine($"Thrust: {this.HookComputer.ShipThrust(this.SensorFleets?.MyFleet?.Ships.Count ?? 0)}");


            await base.AliveAsync();
        }

        protected override Task OnNewLeaderboardAsync()
        {

            return Task.FromResult(0);
        }

        private void ShootAtFleet(Fleet f)
        {
            ShootAt(
                RoboMath.FiringIntercept(
                    HookComputer,
                    this.Position,
                    f.Center,
                    f.Momentum,
                    this.SensorFleets.MyFleet?.Ships.Count ?? 0
                )
            );
        }

        /*public Vector PredictPosition(Fleet fleet, float steeringAngle, int ms)
        {

            var thrust = new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * ThrustAmount;

            Momentum = (Momentum + thrust) * Drag;

        }*/
    }
}
