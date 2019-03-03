namespace Game.Engine.Auditing
{
    using Game.Engine.Core;
    using System;

    public class AuditEventBase
    {
        public string Type { get => this.GetType().Name; }
        public DateTime Created { get; set; }
        public string PublicURL { get; set; }
        public string WorldKey { get; set; }
        public string GameID { get; set; }
        public int AdvertisedPlayerCount { get; set; }
        public long GameTime { get; set; }

        public AuditEventBase()
        { }

        public AuditEventBase(World world)
        {
            this.Created = DateTime.Now;
            if (world != null)
            {
                this.PublicURL = world.GameConfiguration.PublicURL;
                this.WorldKey = world.WorldKey;
                this.AdvertisedPlayerCount = world.AdvertisedPlayerCount;
                this.GameID = world.GameID;
                this.GameTime = world.Time;
            }
        }
    }
}
