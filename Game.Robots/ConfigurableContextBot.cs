namespace Game.Robots
{
    using Game.API.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using static Game.Robots.ConfigurableContextBotConfig;

    public class ConfigurableContextBot : ContextRobot
    {
        protected FileSystemWatcher Watcher;

        public string ConfigurationFileName { get; set; } = null;
        public string ConfigurationFileUrl { get; set; } = null;

        protected long ReloadConfigAfter = 0;
        public int ReloadUrlCycle { get; set; } = 0;

        protected int CurrentLevel { get; set; } = 0;
        public LevelingConfig Leveling { get; set; }
        private bool DownLeveling = false;
        private bool Initialized = false;

        public override Task StartAsync(PlayerConnection connection, CancellationToken cancellationToken = default)
        {
            if (!Initialized)
                InitializeConfiguration();

            return base.StartAsync(connection, cancellationToken);
        }

        public void InitializeConfiguration()
        {
            Initialized = true;
            if (ConfigurationFileName != null)
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
            else if (ConfigurationFileUrl != null)
                LoadConfig();
        }

        protected void LoadConfig()
        {
            try
            {
                string text = null;
                if (ConfigurationFileName != null)
                {
                    text = File.ReadAllText(ConfigurationFileName);
                    ReloadConfigAfter = 0;
                }

                if (ConfigurationFileUrl != null)
                {
                    using (var webClient = new WebClient())
                        text = webClient.DownloadString(ConfigurationFileUrl);
                }

                var config = JsonConvert.DeserializeObject<ConfigurableContextBotConfig>(text);
                SetBehaviors(config.Behaviors);
                if (config.Allies != null)
                    SensorAllies.AlliedNames = config.Allies;

                JsonConvert.PopulateObject(text, this);

                LoadLevel();
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

        protected override Task OnDeathAsync()
        {
            base.OnDeathAsync();
            if (this.Leveling != null)
            {
                if ((this.Leveling.Levels?.Length ?? 0) > (this.CurrentLevel + 1))
                {
                    if (!DownLeveling)
                        CurrentLevel++;
                    LoadLevel();
                }
                DownLeveling = false;

            }

            return Task.FromResult(0);
        }

        protected void LoadLevel()
        {
            if (this.Leveling == null)
                return;

            var level = this.Leveling.Levels[CurrentLevel];
            var json = JsonConvert.SerializeObject(level, Formatting.Indented);
            JsonConvert.PopulateObject(json, this);

            var jobject = level as JObject;

            if (jobject["BehaviorsModifications"] is JObject modifications)
                foreach (var modification in modifications)
                {
                    var behaviorIndex = int.Parse(modification.Key);
                    var behaviorJson = JsonConvert.SerializeObject(modification.Value, Formatting.Indented);
                    JsonConvert.PopulateObject(behaviorJson, this.ContextBehaviors[behaviorIndex]);
                }

        }

        protected async override Task AliveAsync()
        {
            if (ConfigurationFileUrl != null && ReloadUrlCycle > 0 && ReloadConfigAfter == 0)
                ReloadConfigAfter = GameTime + ReloadUrlCycle;

            if (ReloadConfigAfter > 0 && ReloadConfigAfter < GameTime)
            {
                if (ReloadUrlCycle > 0)
                    ReloadConfigAfter = GameTime + ReloadUrlCycle;
                LoadConfig();
            }


            if (Leveling != null && GameTime - SpawnTime > Leveling.DownlevelThresholdMS && CurrentLevel > 0)
            {
                if (!DownLeveling)
                {
                    CurrentLevel--;
                    DownLeveling = true;
                    await Exit();
                }
            }

            await base.AliveAsync();
        }



        public static async Task<ConfigurableContextBot> Load(string path)
        {
            Uri uri = new Uri(path);

            Type robotType = typeof(ConfigurableContextBot);
            //var text = await UriTools.LoadStringAsync(uri);
            //var config = JsonConvert.DeserializeObject<ConfigurableContextBotConfig>(text);

            var config = await UriTools.LoadAsync<ConfigurableContextBotConfig>(path);
            if (config.RobotType != null)
                robotType = Type.GetType(config.RobotType);

            var robot = Activator.CreateInstance(robotType) as ConfigurableContextBot;

            if (uri.Scheme == "file")
                robot.ConfigurationFileName = uri.LocalPath;
            else
                robot.ConfigurationFileUrl = uri.ToString();

            robot.InitializeConfiguration();

            return robot;
        }
    }
}
