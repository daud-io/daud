namespace Game.Engine.Core
{
    public interface IActor
    {
        void Init(World world);
        void Deinit();
        void Step();
    }
}
