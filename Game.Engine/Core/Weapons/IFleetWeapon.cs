namespace Game.Engine.Core.Weapons
{
    public interface IFleetWeapon
    {
        void FireFrom(Fleet fleet);
        bool IsOffense { get; }
        bool IsDefense { get; }
    }
}
