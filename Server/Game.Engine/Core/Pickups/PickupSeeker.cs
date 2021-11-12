namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;

    public class PickupSeeker : PickupBase
    {
        public PickupSeeker(World world): base(world)
        {
            Size = 100;
            Sprite = Sprites.seeker_pickup;
        }

        protected override void EquipFleet(Fleet fleet)
        {
            fleet.PushStackWeapon(new FleetWeaponGeneric<ShipWeaponSeeker>(fleet.World) {
                IsOffense = true
            });

            fleet.ShootCooldownTime = World.Time;
        }
    }
}
