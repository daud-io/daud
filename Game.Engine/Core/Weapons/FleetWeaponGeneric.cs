namespace Game.Engine.Core.Weapons
{
    using System;

    public class FleetWeaponGeneric<T> : IFleetWeapon
        where T : IShipWeapon
    {
        public bool IsOffense { get; set; }
        public bool IsDefense { get; set; }

        public bool CausesCooldown {get; set; } = true;

        public FleetWeaponGeneric(World world, Action<T> configure = null)
        {
        }
        public void FireFrom(Fleet fleet)
        {
            ShipWeaponVolley<T>.FireFrom(fleet);
        }
    }
}
