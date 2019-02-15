namespace Game.Robots
{
    using Game.Robots.Targeting;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class ConfigurableContextBot : ContextRobot
    {
        private readonly FleetTargeting FleetTargeting;
        private readonly AbandonedTargeting AbandonedTargeting;
        private readonly FishTargeting FishTargeting;
        private FileSystemWatcher Watcher;

        public string ConfigurationFileName { get; set; } = "config.json";

        public ConfigurableContextBot()
        {
            FleetTargeting = new FleetTargeting(this);
            AbandonedTargeting = new AbandonedTargeting(this);
            FishTargeting = new FishTargeting(this);

            InitializeConfiguration();

            Steps = 16;
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

            this.CustomData = JsonConvert.SerializeObject(new
            {
                plotly = new {
                    /*data = new[] {
                        new {
                            r = this.Behaviors[1]?.LastRing?.Weights,
                            name = "dodge",
                            color = "rgb(100,201,226)",
                            type = "barpolar"
                        },
                        new {
                            r = this.Behaviors[2]?.LastRing?.Weights,
                            name = "dodge",
                            color = "rgb(100,201,226)",
                            type = "barpolar"
                        },
                        new {
                            r = this.Behaviors[3]?.LastRing?.Weights,
                            name = "dodge",
                            color = "rgb(100,201,226)",
                            type = "barpolar"
                        }
                    },*/

                    data = this.Behaviors.Where(b => b.Plot).Select(b => new
                    {
                        r = b.LastRing?.Weights.Select(w => w * b.BehaviorWeight).Reverse(),
                        name = b.LastRing?.Name,
                        opacity = 0.5,
                        type = "barpolar"
                    }),
                    layout = new
                    {
                        paper_bgcolor = "rgba(0,0,0,0)",
                        plot_bgcolor = "rgba(0,0,0,0)",
                        hovermode = false,
                        polar = new
                        {
                            bgcolor = "rgba(0,0,0,0)",
                            angularaxis = new
                            {
                                rotation = 180
                            }
                        }
                    }
                }
            });

            if (CanShoot)
            {
                var target = FleetTargeting.ChooseTarget()
                    ?? AbandonedTargeting.ChooseTarget()
                    ?? FishTargeting.ChooseTarget();

                if (target != null)
                    ShootAt(target.Position);
            }

            if (CanBoost && (this.SensorFleets.MyFleet?.Ships.Count ?? 0) > 16)
                Boost();

            await base.AliveAsync();
        }
    }
}
