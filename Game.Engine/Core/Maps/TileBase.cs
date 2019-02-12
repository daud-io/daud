namespace Game.Engine.Core.Maps
{
    public class TileBase : ActorBody
    {
        public TileBase()
        {
            MaximumCleanTime = 100000;
            IsStatic = true;
        }
    }
}
