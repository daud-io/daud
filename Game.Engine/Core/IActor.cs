namespace Game.Engine.Core
{
    public interface IActor
    {
        void Destroy();

        void Think(float dt);
        void Cleanup();
    }
}
