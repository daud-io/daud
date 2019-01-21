namespace Game.Engine.Core.Weapons
{
    public class FleetWeaponBullet : IFleetWeapon
    {
        public void FireFrom(Fleet fleet)
        {
            ShipWeaponVolley<Bullet>.FireFrom(fleet);
        }
    }
}
