namespace Game.Robots.Models
{
    using System.Numerics;

    public class Ship
    {
        public uint ID { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Momentum { get; set; }
        public int Size { get; set; }
        public float Angle { get; set; }

        public bool PendingDestruction { get; set; }
        public Ship Clone()
        {
            Ship ns = new Ship();
            ns.ID = this.ID;
            ns.Size = this.Size;
            ns.Position = new Vector2(this.Position.X, this.Position.Y);
            ns.Momentum = new Vector2(this.Momentum.X, this.Momentum.Y);
            ns.Angle = this.Angle;
            return ns;
        }
    }
}
