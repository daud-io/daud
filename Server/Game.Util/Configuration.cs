namespace Game.Util
{
    using Game.Util.Commands;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    static class Configuration
    {
        private const string PROFILE_SUBFOLDER = ".game";
        private const string CONFIG_FILE = "config.json";

        public static string ConfigurationFilePath()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile
                ),
                PROFILE_SUBFOLDER);

            return Path.Combine(path, CONFIG_FILE);
        }

        public static void Save(ConfigurationRoot.UtilConfiguration configuration)
        {
            var fileName = ConfigurationFilePath();
            var path = Path.GetDirectoryName(fileName);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.WriteAllText(fileName, JsonConvert.SerializeObject(configuration, Formatting.Indented));
        }

        public static (ConfigurationRoot.UtilConfiguration, ConfigurationRoot.UtilConfiguration.GameEngineConnectionConfiguration) Load(RootCommand root)
        {

            var config = new ConfigurationRoot
            {
                Config = new ConfigurationRoot.UtilConfiguration
                {
                    Contexts = new Dictionary<string, ConfigurationRoot.UtilConfiguration.GameEngineConnectionConfiguration>()
                }
            };

            var globalConfigPath = System.Environment.GetEnvironmentVariable("GAME_CONFIG_PATH");
            if (globalConfigPath != null)
                if (File.Exists(globalConfigPath))
                    JsonConvert.PopulateObject(File.ReadAllText(globalConfigPath), config);

            var userConfigPath = ConfigurationFilePath();
            if (File.Exists(userConfigPath))
                JsonConvert.PopulateObject(File.ReadAllText(userConfigPath), config.Config);

            ConfigurationRoot.UtilConfiguration.GameEngineConnectionConfiguration context = null;
            var activeContextName = config.Config.CurrentContext;

            if (root.UseContext != null)
            {
                if (config.Config.Contexts.ContainsKey(root.UseContext))
                    activeContextName = root.UseContext;
                else
                    throw new Exception("Configuration error, specified Context does not exist");
            }

            if (config.Config.Contexts.Any())
            {
                if (config.Config.CurrentContext != null && config.Config.Contexts.ContainsKey(activeContextName))
                    context = config.Config.Contexts[activeContextName];
                else
                    throw new Exception("Configuration error, CurrentContext not in Contexts list");
            }
            else
                context = new ConfigurationRoot.UtilConfiguration.GameEngineConnectionConfiguration
                {
                    Uri = root.Server,
                    UserKey = root.UserKey,
                    Password = root.Password
                };

            return (config.Config, context);
        }
    }
}
