namespace Game.Robots
{
    using Game.API.Client;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class ConfigurableContextBot : ContextRobot
    {
        protected FileSystemWatcher Watcher;

        public string ConfigurationFileName { get; set; } = "config.json";

        protected long ReloadConfigAfter = 0;

        public override Task StartAsync(Connection connection)
        {
            InitializeConfiguration();
            return base.StartAsync(connection);
        }

        private void InitializeConfiguration()
        {
            var fileName = Path.GetFullPath(ConfigurationFileName);

            Watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(fileName),
                Filter = Path.GetFileName(fileName)
            };
            Watcher.Changed += Watcher_Changed;
            Watcher.Created += Watcher_Changed;
            Watcher.NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.Security;
            Watcher.EnableRaisingEvents = true;
            LoadConfig();
        }

        protected void LoadConfig()
        {
            try
            {
                var text = File.ReadAllText(ConfigurationFileName);
                var config = JsonConvert.DeserializeObject<ConfigurableContextBotConfig>(text);

                SetBehaviors(config.Behaviors);
                JsonConvert.PopulateObject(text, this);


                JsonConvert.PopulateObject(JsonConvert.SerializeObject(config.BlendingConfig), ContextRingBlending);

            }
            catch (IOException) { }
            catch (Exception e)
            {
                this.Log("Failed to read configuration: " + e);
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            ReloadConfigAfter = GameTime + 500;
            
        }

        protected async override Task AliveAsync()
        {
            if (ReloadConfigAfter > 0 && ReloadConfigAfter < GameTime)
            {
                LoadConfig();
                ReloadConfigAfter = 0;
            }

            await base.AliveAsync();
        }
    }
}
