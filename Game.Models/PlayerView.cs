namespace Game.Models
{
    using Game.Models.Messages;
    using System.Collections.Generic;
    using System.Numerics;

    public class PlayerView
    {
        public long Time { get; set; }
        public int PlayerCount { get; set; }

        public long DefinitionTime { get; set; }
        public Vector2? OriginalPosition { get; set; }
        public Vector2? Momentum { get; set; }

        public bool IsAlive { get; set; }
        public float Health { get; set; }

        public Leaderboard Leaderboard { get; set; }

        public IEnumerable<ProjectedBody> Updates { get; set; }
        public IEnumerable<long> Deletes { get; set; }

        public IEnumerable<string> Messages { get; set; }

        public Hook Hook { get; set; }
    }
}