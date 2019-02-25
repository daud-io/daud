namespace Game.Engine.Core.Weapons
{
    using Game.Engine.Core.Pickups;

    public class FleetWeaponShieldCannon : IFleetWeapon
    {
        public bool IsOffense => false;
        public bool IsDefense => true;

        public void FireFrom(Fleet fleet)
        {
            PickupBase.FireFrom<PickupShield>(fleet);
        }
    }
}
