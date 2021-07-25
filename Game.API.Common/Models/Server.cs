using System.Collections.Generic;

namespace Game.API.Common.Models
{
    public class Server
    {
        public int WorldCount { get; set; }
        public int PlayerCount { get; set; }

        public List<Callout> Callouts {get;set;}

        public class Callout
        {
            public string Name {get;set;}
            public string AvatarUrl {get;set;}
        }
    }
}
