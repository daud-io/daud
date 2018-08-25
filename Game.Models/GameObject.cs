namespace Game.Models
{
    using System.Numerics;

    public class GameObject
    {
        private static long NextID = 0;

        public GameObject()
        {
            ID = NextID++;
        }

        public long ID { get; set; }
        public Vector2 Position { get; set; } = new Vector2(0, 0);
        public Vector2 LastPosition { get; set; } = new Vector2(0, 0);
        public Vector2 Momentum { get; set; } = new Vector2(0, 0);
        public string ObjectType { get; set; } = null;
        public float Angle { get; set; } = 0;
        public string Sprite { get; set; } = null;
        public string Caption { get; set; } = null;
        public float Health { get; set; } = 0;
        public int Size { get; set; } = 0;

        public bool Exists { get; set; } = false;
    }
}
