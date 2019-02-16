namespace Game.Robots
{
    using Game.Robots.Targeting;
    using System.Threading.Tasks;

    public class ConfigTurret : ConfigurableContextBot
    {
        private readonly FleetTargeting FleetTargeting;
        private readonly AbandonedTargeting AbandonedTargeting;
        private readonly FishTargeting FishTargeting;

        public ConfigTurret()
        {
            FleetTargeting = new FleetTargeting(this);
            AbandonedTargeting = new AbandonedTargeting(this);
            FishTargeting = new FishTargeting(this);
        }

        protected async override Task AliveAsync()
        {
            await base.AliveAsync();

            if (CanShoot)
            {
                var target = FleetTargeting.ChooseTarget()
                    ?? AbandonedTargeting.ChooseTarget()
                    ?? FishTargeting.ChooseTarget();

                if (target != null)
                    ShootAt(target.Position);
            }

            if (CanBoost && (this.SensorFleets.MyFleet?.Ships.Count ?? 0) > 16)
                Boost();

        }
    }
}
