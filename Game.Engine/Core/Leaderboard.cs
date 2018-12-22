namespace Game.Engine.Core
{
    using System.Collections.Generic;
    using System.Numerics;

    public class Leaderboard
    {
        public List<Entry> Entries { get; set; }
        public long Time { get; set; } = 0;
        public string Type { get; set; }

        public Entry ArenaRecord { get; set; }

        public class Entry
        {
            public int Score { get; set; }
            public string Name { get; set; }
            public string Color { get; set; } = "white";
            public Vector2 Position { get; set; }
            public string Token { get; set; }
            public object ModeData { get; set; }
        }
    }
}