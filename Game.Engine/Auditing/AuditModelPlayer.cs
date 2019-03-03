namespace Game.Engine.Auditing
{
    using Game.Engine.Core;

    public class AuditModelPlayer
    {
        public ulong LoginID { get; }
        public string LoginName { get; set; }
        public uint FleetID { get; set; }
        public string FleetName { get; set; }
        public int FleetSize { get; set; }
        public int Score { get; set; }
        public long AliveSince { get; set; }
        public int ComboCounter { get; set; }
        public uint Latency { get; set; }

        public AuditModelPlayer(Player player)
        {
            if (player == null)
                return;

            this.LoginID = player.LoginID;
            this.LoginName = player.LoginName;
            this.FleetID = player.Fleet?.ID ?? 0;
            this.FleetName = player.Name;
            this.FleetSize = player.Fleet?.Ships?.Count ?? 0;
            this.Score = player.Score;
            this.AliveSince = player.AliveSince;
            this.ComboCounter = player.ComboCounter;
            this.Latency = player.Connection?.Latency ?? 0;
        }
    }
}
