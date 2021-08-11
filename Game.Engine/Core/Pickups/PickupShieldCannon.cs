namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;

    public class PickupShieldCannon : PickupBase
    {
        public PickupShieldCannon(World world): base(world)
        {
            Size = 150;
            Sprite = Sprites.shield_pickup;
        }

        protected override void EquipFleet(Fleet fleet)
        {
            fleet.PushStackWeapon(new FleetWeaponPickupCannon<PickupShield>());
            Die();
        }
    }
}
