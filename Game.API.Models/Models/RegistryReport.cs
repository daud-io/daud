namespace Game.API.Common.Models
{
    using System;
    using System.Collections.Generic;

    public class RegistryReport
    {
        public List<World> Worlds { get; set; }
        public string URL { get; set; }

        public DateTime Received { get; set; }

        public class World
        {
            public Hook Hook { get; set; }
            public string WorldKey { get; set; }
            public int AdvertisedPlayers { get; set; }
        }
    }
}
