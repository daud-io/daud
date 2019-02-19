namespace Game.Util
{
    using System.Collections.Generic;

    public class ConfigurationRoot
    {
        public UtilConfiguration Config { get; set; }

        public class UtilConfiguration
        {
            public string CurrentContext { get; set; }

            public Dictionary<string, GameEngineConnectionConfiguration> Contexts { get; set; }

            public class GameEngineConnectionConfiguration
            {
                public string Uri { get; set; }
                public string UserKey { get; set; }
                public string Password { get; set; }

                public string RegistryUri { get; set; }
                public string RegistryUserKey { get; set; }
                public string RegistryPassword { get; set; }
            }
        }
    }
}