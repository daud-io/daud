namespace Game.API.Common.Models.Auditing
{
    using System;

    public class AuditEventBase
    {
        public string Type { get; set; }
        public DateTime Created { get; set; }
        public string PublicURL { get; set; }
        public string WorldKey { get; set; }
        public string GameID { get; set; }
        public int AdvertisedPlayerCount { get; set; }
        public long GameTime { get; set; }
    }
}
