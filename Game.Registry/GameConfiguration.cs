namespace Game.Registry
{
    public class GameConfiguration
    {
        public string TokenValidationSecret { get; set; } = null;
        public string TokenIssuer { get; set; } = "game";
        public string TokenAudience { get; set; } = "players";
        public int TokenExpirationSeconds { get; set; } = 100000;
        public string AdministratorPassword { get; set; }

        public bool AllowCORS { get; set; } = true;
        public string ElasticSearchURI { get; set; }

        public bool DisableSuggestionLookup {get; set;} = false;
    }
}
