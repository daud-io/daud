namespace Game.Engine.Core.Weapons
{
    using Game.Engine.Core.Pickups;

    public class FleetWeaponPickupCannon<T> : IFleetWeapon
        where T : PickupBase
    {
        public bool IsOffense => false;
        public bool IsDefense => true;

        public void FireFrom(Fleet fleet)
        {
            var pickup = PickupBase.FireFrom<T>(fleet);
            pickup.TimeDeath = fleet.World.Time + fleet.World.Hook.ShieldCannonballLife;
        }
    }
}
