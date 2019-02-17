namespace Game.Robots
{
    using Game.Robots.Targeting;
    using System.Threading.Tasks;

    public class ConfigTurret : ConfigurableContextBot
    {
        private readonly FleetTargeting FleetTargeting;
        private readonly AbandonedTargeting AbandonedTargeting;
        private readonly FishTargeting FishTargeting;
        public bool AttackFleets=true;
        public bool AttackFish=true;
        public bool AttackAbandoned=true;

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
                var target = (AttackFleets?FleetTargeting.ChooseTarget():null)
                    ?? (AttackAbandoned?AbandonedTargeting.ChooseTarget():null)
                    ?? (AttackFish?FishTargeting.ChooseTarget():null);

                if (target != null)
                    ShootAt(target.Position);
            }

            if (CanBoost && (this.SensorFleets.MyFleet?.Ships.Count ?? 0) > 16)
                Boost();

        }
    }
}
