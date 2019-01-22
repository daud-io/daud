namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;

    public class PickupSeeker : PickupBase
    {
        public PickupSeeker()
        {
            Size = 100;
            Sprite = Sprites.seeker_pickup;
        }

        protected override void EquipFleet(Fleet fleet)
        {
            fleet.PushStackWeapon(new FleetWeaponGeneric<ShipWeaponSeeker>());
        }
    }
}
