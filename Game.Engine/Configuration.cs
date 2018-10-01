namespace Game.Engine
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;

    public class Configuration
    {
        protected static bool LoggingInitialized = false;
    }

    public class Configuration<T> : Configuration
        where T : class, new()
    {
        public IConfigurationRoot ConfigurationRoot { get; internal set; }
        public T Object { get; set; }

        public static Configuration<T> Load(string sectionName, string basePath = null, T instance = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath ?? Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Console.WriteLine("basePath: " + basePath);

            var configPath = System.Environment.GetEnvironmentVariable("GAME_CONFIG_PATH");
            if (configPath != null)
            {
                if (!File.Exists(configPath))
                    Console.WriteLine($"Warning: Environment variable GAME_CONFIG_PATH is set to {configPath}, but the file does not seem to exist");
                else
                {
                    Console.WriteLine($"Considering Config at {configPath}");
                    builder = builder.AddJsonFile(configPath, optional: true, reloadOnChange: true);
                }
            }

            builder.AddEnvironmentVariables();

            var config = new Configuration<T>
            {
                ConfigurationRoot = builder.Build(),
                Object = instance ?? new T()
            };

            builder.Build()
                .GetSection(sectionName)
                .Bind(config.Object);

            return config;
        }

        public IConfigurationSection GetSection(string sectionName)
        {
            return this.ConfigurationRoot.GetSection(sectionName);
        }
    }
}
