namespace Game.Engine.Core.Weapons
{
    public interface IShipWeapon
    {
        void FireFrom(Ship ship, ActorGroup group);
        bool Active { get; }
        void Init(World world);
    }
}
