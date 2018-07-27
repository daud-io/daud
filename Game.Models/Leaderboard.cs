namespace Game.Models
{
    using System.Collections.Generic;

    public class Leaderboard
    {
        public List<Entry> Entries { get; set; }

        public class Entry
        {
            public int Score { get; set; }
            public string Name { get; set; }
        }
    }
}