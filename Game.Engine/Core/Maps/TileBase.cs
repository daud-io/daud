namespace Game.Engine.Core.Maps
{
    public class TileBase : ActorBody
    {
        public WorldMap WorldMap { get; internal set; }
        public float Drag { get; set; }

        public TileBase()
        {
            MaximumCleanTime = 100000;
            IsStatic = true;
        }

        public virtual void InteractWithShip(Ship ship)
        {
            if (Drag != 0 && ship.Momentum.Length() < 5)
                ship.Momentum *= Drag;
        }
    }
}
