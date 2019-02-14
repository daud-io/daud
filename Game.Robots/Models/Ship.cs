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
    }
}
