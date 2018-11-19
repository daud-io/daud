namespace Game.API.Client
{
    public class APIEndpoint
    {
        public APIEndpoint(string endpoint)
        {
            Endpoint = endpoint;
        }

        public virtual string Endpoint { get; private set; }

        public static APIEndpoint UserAuthenticate { get => new APIEndpoint("/api/v1/user/authenticate"); }
        public static APIEndpoint ServerGet { get => new APIEndpoint("/api/v1/server"); }
        public static APIEndpoint ServerReset { get => new APIEndpoint("/api/v1/server/reset"); }
        public static APIEndpoint ServerAnnounce { get => new APIEndpoint("/api/v1/server/announce"); }
        public static APIEndpoint ServerPlayers { get => new APIEndpoint("/api/v1/server/players"); }
        public static APIEndpoint PlayerConnect { get => new APIEndpoint("/api/v1/connect"); }
    }
}