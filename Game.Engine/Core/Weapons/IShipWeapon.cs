namespace Game.Engine.Core.Weapons
{
    public interface IShipWeapon
    {
        void FireFrom(Ship ship, ActorGroup group);
        bool Active();
    }
}
