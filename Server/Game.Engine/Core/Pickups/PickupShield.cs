namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;

    public class PickupShield : PickupBase
    {
        public PickupShield(World world): base(world)
        {
            Size = 100;
            Sprite = Sprites.shield_pickup;
        }

        protected override void EquipFleet(Fleet fleet)
        {
            fleet.PushStackWeapon(new FleetWeaponShield());
            fleet.ShootCooldownTime = World.Time;
        }
    }
}
