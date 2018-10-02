namespace Game.Engine.Core
{
    public interface IActor
    {
        void Init(World world);
        void Destroy();

        void Think();
        void CreateDestroy();
    }
}
