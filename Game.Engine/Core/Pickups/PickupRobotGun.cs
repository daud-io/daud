namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;

    public class PickupRobotGun : PickupBase
    {
        public PickupRobotGun()
        {
            Size = 100;
            Sprite = Sprites.ship0;
        }

        protected override void EquipFleet(Fleet fleet)
        {
            fleet.PushStackWeapon(new FleetWeaponRobot());
        }
    }
}
