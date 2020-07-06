namespace Game.Robots.Monsters
{
    using Game.API.Client;
    using Game.API.Common;
    using Game.Robots.Herding;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    public class ConfigMonster : ConfigTurret
    {
        private object ShipTemplate = null;
        private long lastShot = 0;
        
        public int MaxShips { get; set; } = 2;
        public Vector2? NextPosition { get; set; } = null;

        public int ShipSize { get; set; } = 80;

        public readonly Shepherd Tender;
        public bool DestroyOnDeath { get; set; }

        public ConfigMonster(Shepherd tender = null)
        {
            Tender = tender;
        }

        protected override Task AliveAsync()
        {
            if (SensorFleets.MyFleet?.Ships?.Any() == true)
            {
                SetShipTemplate(ShipTemplate);
                NextPosition = null;
            }
            else
                SetShipTemplate(null);

            return base.AliveAsync();
        }

        private void SetShipTemplate(object template)
        {
            if (SensorFleets?.MyFleet?.Ships?.Count > 0)
            {
                var ships = new string[SensorFleets.MyFleet.Ships.Count];
                for (int i = 0; i < ships.Length; i++)
                    ships[i] = JsonConvert.SerializeObject(new
                        {
                            Abandoned = i >= MaxShips,
                            Size = ShipSize,
                            Sprite = Enum.Parse<Sprites>(Sprite),
                            //ThrustOverride = 0,
                            Position = NextPosition
                        });

                CustomData = JsonConvert.SerializeObject(new
                {
                    Magic = JsonConvert.SerializeObject(new
                    {
                        //IsShielded = true,
                        ShipSprite = Enum.Parse<Sprites>(Sprite),
                        Fleet = new
                        {
                            //Burden = 1,
                            ShipConfig = ships
                        },
                    })
                });
            }
        }

        public void Configure(string config)
        {
            if (Uri.IsWellFormedUriString(config, UriKind.Absolute))
                this.ConfigurationFileUrl = config;
            else
                this.ConfigurationFileName = config;

            InitializeConfiguration();
        }

        protected async override Task OnDeathAsync()
        {
            await base.OnDeathAsync();
            if (Tender != null)
                await Tender.OnSheepDeath(this);
        }

        protected  async Task<T> SpawnChildAsync<T>(string config = null, Vector2 relativePosition = default)
            where T: ConfigMonster
        {
            var child = Activator.CreateInstance(typeof(T), Tender) as T;
            if (this.SensorFleets?.MyFleet != null)
                child.NextPosition = this.SensorFleets.LastKnownCenter + relativePosition;

            if (config != null)
                child.Configure(config);

            await Tender.StartRobot(child);
            await child.SpawnAsync();

            return child;
        }
    }
}
