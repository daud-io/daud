namespace Game.Engine.Core.Weapons
{
    using System;

    public class FleetWeaponGeneric<T> : IFleetWeapon
        where T : IShipWeapon, new()
    {

        public FleetWeaponGeneric(Action<T> configure = null)
        {

        }
        public void FireFrom(Fleet fleet)
        {
            ShipWeaponVolley<T>.FireFrom(fleet);
        }
    }
}
