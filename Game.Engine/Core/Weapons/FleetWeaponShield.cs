namespace Game.Engine.Core.Weapons
{
    public class FleetWeaponShield : IFleetWeapon
    {
        public void FireFrom(Fleet fleet)
        {
            fleet.Owner?.SetInvulnerability(fleet.World.Hook.SpawnInvulnerabilityTime);
        }
    }
}
