﻿namespace Game.Engine
{
    public class GameConfiguration
    {
        public string TokenValidationSecret { get; set; } = null;
        public string TokenIssuer { get; set; } = "game";
        public string TokenAudience { get; set; } = "players";
        public int TokenExpirationSeconds { get; set; } = 100000;
        public string AdministratorPassword { get; set; }

        public string DiscordToken { get; set; } = null;
        public ulong? DiscordGuildID { get; set; } = null;

        public bool AllowCORS { get; set; } = true;

        public bool NoWorlds { get; set; } = false;

        public WorldConfiguration[] WorldInitialization { get; set; } = null;
        public bool RegistryEnabled { get; set; }
        public string RegistryUri { get; set; }
        public string RegistryUserKey { get; set; }
        public string RegistryPassword { get; set; }

        public string PublicURL { get; set; }

        public string TelemetryFile { get; set; } = null;

        public class WorldConfiguration
        {
            public string WorldKey { get; set; }
            public string HookURL { get; set; }

        }
    }
}
