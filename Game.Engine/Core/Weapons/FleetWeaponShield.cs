namespace Game.Engine.Core.Weapons
{
    public class FleetWeaponShield : IFleetWeapon
    {
        public bool IsOffense => false;
        public bool IsDefense => true;
        public bool CausesCooldown => true;

        public void FireFrom(Fleet fleet)
        {
            fleet.ActivateShields();
        }
    }
}
