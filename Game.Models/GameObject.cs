using System.Numerics;

namespace Game.Models
{
    public class GameObject
    {
        public Vector2 Position { get; set; } = new Vector2(0, 0);
        public Vector2 Momentum { get; set; } = new Vector2(0, 0);
    }
}
