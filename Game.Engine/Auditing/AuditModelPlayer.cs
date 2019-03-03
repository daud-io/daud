namespace Game.Engine.Auditing
{
    using Game.Engine.Core;
    using System.Numerics;

    public class AuditModelPlayer
    {
        public ulong LoginID { get; }
        public string LoginName { get; set; }

        public string PlayerID { get; set; }
        public uint FleetID { get; set; }
        public string FleetName { get; set; }

        public int FleetSize { get; set; }
        public int Score { get; set; }
        public long AliveSince { get; set; }
        public int ComboCounter { get; set; }
        public int MaxCombo { get; set; }
        public uint Latency { get; set; }
        public int KillCount { get; set; }
        public int KillStreak { get; set; }

        public Vector2? Position { get; set; }
        public Vector2? Momentum {get; set;}

        public AuditModelPlayer(Player player)
        {
            if (player == null)
                return;

            this.LoginID = player.LoginID;
            this.LoginName = player.LoginName;
            this.PlayerID = player.PlayerID;
            this.FleetID = player.Fleet?.ID ?? 0;
            this.FleetName = player.Name;
            this.FleetSize = player.Fleet?.Ships?.Count ?? 0;
            this.Score = player.Score;
            this.AliveSince = player.AliveSince;
            this.Latency = player.Connection?.Latency ?? 0;
            this.KillCount = player.KillCount;
            this.KillStreak = player.KillStreak;
            this.ComboCounter = player.ComboCounter;
            this.MaxCombo = player.MaxCombo;

            this.Position = player.Fleet?.FleetCenter;
            this.Momentum = player.Fleet?.FleetMomentum;
        }
    }
}
