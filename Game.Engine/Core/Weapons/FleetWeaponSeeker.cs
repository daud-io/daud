namespace Game.Engine.Core.Weapons
{
    public class FleetWeaponSeeker : IFleetWeapon
    {
        public void FireFrom(Fleet fleet)
        {
            ShipWeaponVolley<ShipWeaponSeeker>.FireFrom(fleet);
        }
    }
}
