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
        public static APIEndpoint ServerHook { get => new APIEndpoint("/api/v1/server/hook"); }
        public static APIEndpoint World { get => new APIEndpoint("/api/v1/world"); }
        public static APIEndpoint WorldHook { get => new APIEndpoint("/api/v1/world/hook"); }
        public static APIEndpoint WorldReset { get => new APIEndpoint("/api/v1/world/reset"); }
        public static APIEndpoint WorldMap { get => new APIEndpoint("/api/v1/world/map"); }
        public static APIEndpoint Registry { get => new APIEndpoint("/api/v1/registry"); }
        public static APIEndpoint RegistrySuggestion { get => new APIEndpoint("/api/v1/registry/suggestion"); }
        public static APIEndpoint RegistryReport { get => new APIEndpoint("/api/v1/registry/report"); }
        public static APIEndpoint Events { get => new APIEndpoint("/api/v1/registry/events"); }

        public static APIEndpoint PlayerConnect(string worldName = null)
        {
            return new APIEndpoint($"/api/v1/connect?world={worldName}");
        }
    }
}