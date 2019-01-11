namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Newtonsoft.Json;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class ContextTurret : ContextRobot
    {
        protected readonly NavigateToPoint Navigation;

        public ContextTurret(Vector2 target)
        {
            Behaviors.Add(Navigation = new NavigateToPoint(this) { BehaviorWeight = 0.00f });
            Behaviors.Add(new Efficiency(this) { BehaviorWeight = 0.1f });
            Behaviors.Add(new Dodge(this) { LookAheadMS = 100, BehaviorWeight = 4 });
            Behaviors.Add(new Dodge(this) { LookAheadMS = 250, BehaviorWeight = 2 });
            Behaviors.Add(new Dodge(this) { LookAheadMS = 500, BehaviorWeight = 1 });
            Behaviors.Add(new StayInBounds(this) { BehaviorWeight = 1f });

            Navigation.TargetPoint = target;
            Steps = 16;
        }

        protected async override Task AliveAsync()
        {
            if (CanShoot)
            {
                var closest = SensorFleets.Others
                    .OrderBy(f => Vector2.Distance(this.Position, f.Center))
                    .FirstOrDefault();

                if (closest != null)
                {
                    ShootAt(closest.Center);

                }
            }

            if (SensorFleets.Others != null)
                CustomData = JsonConvert.SerializeObject(new
                {
                    spots = SensorFleets.Others.Select(f => f.Center)
                });

            await base.AliveAsync();
        }
    }
}
