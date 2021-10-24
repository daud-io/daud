namespace Game.API.Common.Models.Auditing
{
    using System.Numerics;

    public class AuditModelPlayer
    {
        public ulong LoginID { get; set; }
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
        public Vector2? Velocity { get; set; }
    }
}
