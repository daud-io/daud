namespace Game.Models
{
    using System.Collections.Generic;
    using System.Numerics;

    public class PlayerView
    {
        public long Time { get; set; }
        public int PlayerCount { get; set; }

        public Vector2? Position { get; set; }

        public IEnumerable<GameObject> Objects { get; set; }
    }
}