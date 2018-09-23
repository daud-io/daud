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
        public static APIEndpoint HealthGet { get => new APIEndpoint("/api/v1/server/health"); }
    }
}