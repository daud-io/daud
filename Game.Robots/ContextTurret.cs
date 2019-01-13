namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Models;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class ContextTurret : ContextRobot
    {
        protected readonly NavigateToPoint Navigation;
        private long CustomDataTime = 0;

        public ContextTurret(Vector2 target)
        {
            Behaviors.Add(Navigation = new NavigateToPoint(this) { BehaviorWeight = 0.00f });
            Behaviors.Add(new Efficiency(this) { BehaviorWeight = 0.1f });
            Behaviors.Add(new Dodge(this) { LookAheadMS = 250, BehaviorWeight = 2 });
            Behaviors.Add(new Dodge(this) { LookAheadMS = 500, BehaviorWeight = 2 });
            Behaviors.Add(new Dodge(this) { LookAheadMS = 1000, BehaviorWeight = 2 });
            Behaviors.Add(new StayInBounds(this) { LookAheadMS = 2000, BehaviorWeight = 1f });

            Navigation.TargetPoint = target;
            Steps = 16;
        }

        protected async override Task AliveAsync()
        {
            if (CanShoot)
            {
                var closest = SensorFleets.Others
                    .OrderBy(f => Vector2.Distance(this.Position, f.Center))
                    .Where(f => f.Name != this.Name)
                    .Where(f => !HookComputer.TeamMode || f.Color != this.Color)
                    .FirstOrDefault();

                if (closest != null)
                    ShootAtFleet(closest);
            }

            if (CanBoost && (SensorFleets.MyFleet?.Ships.Count ?? 0) > 8 )
                Boost();


            /*CustomData = JsonConvert.SerializeObject(new
            {
                spots = Behaviors.OfType<Dodge>().Where(d => d.ConsideredPoints != null).SelectMany(d => d.ConsideredPoints)
            });*/


            CustomData = JsonConvert.SerializeObject(new
            {
                spots = SensorFleets.Others?.Select(f => RoboMath.FiringIntercept(HookComputer, this.Position, f.Center, f.Momentum, this.SensorFleets.MyFleet?.Ships.Count ?? 0))
            });

//            Console.WriteLine($"Thrust: {this.HookComputer.ShipThrust(this.SensorFleets?.MyFleet?.Ships.Count ?? 0)}");

            await base.AliveAsync();
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
