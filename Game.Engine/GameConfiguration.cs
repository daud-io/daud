namespace Game.Engine
{
    public class GameConfiguration
    {
        public string TokenValidationSecret { get; set; } = null;
        public string TokenIssuer { get; set; } = "game";
        public string TokenAudience { get; set; } = "players";
        public int TokenExpirationSeconds { get; set; } = 100000;
        public string AdministratorPassword { get; set; }

        public string DiscordToken {get; set;} = null;

        public bool ForceHTTPS { get; set; } = false;

        public bool AllowCORS {get; set;} = true;
    }
}
