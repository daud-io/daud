namespace Game.Robots
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class ConfigurableContextBot : ContextRobot
    {
        private FileSystemWatcher Watcher;

        public string ConfigurationFileName { get; set; } = "config.json";

        public ConfigurableContextBot()
        {
            InitializeConfiguration();
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

        private void LoadConfig()
        {
            try
            {
                var text = File.ReadAllText(ConfigurationFileName);
                var config = JsonConvert.DeserializeObject<ConfigurableContextBotConfig>(text);

                SetBehaviors(config.Behaviors);
                JsonConvert.PopulateObject(text, this);
            }
            catch (IOException) { }
            catch (Exception e)
            {
                this.Log("Failed to read configuration: " + e);
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            LoadConfig();
        }

        protected async override Task AliveAsync()
        {
            await base.AliveAsync();
        }
    }
}
