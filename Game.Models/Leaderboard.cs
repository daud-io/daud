namespace Game.Models
{
    using System.Collections.Generic;

    public class Leaderboard
    {
        public List<Entry> Entries { get; set; }
        public long Time { get; set; } = 0;

        public class Entry
        {
            public int Score { get; set; }
            public string Name { get; set; }
            public string Color { get; set; } = "white";
        }
    }
}