namespace Game.Robots.Monsters
{
    using Game.API.Client;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    public class ConfigMonster : ConfigurableContextBot
    {
        private object ShipTemplate = null;
        private long lastShot = 0;


        protected override Task AliveAsync()
        {
            if (SensorFleets.MyFleet?.Ships != null)
            {
                SetShipTemplate(ShipTemplate);
            }
            else
                SetShipTemplate(null);


            if (SensorFleets?.Others?.Any() == true && GameTime - lastShot > 300)

            {
                ShootAt(SensorFleets.Others.First().Center);
                lastShot = GameTime;
            }

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
                            Size = 100,
                            Sprite = 24,
                            Health = 1000,
                            //ShieldStrength = 1000,
                            ThrustOverride = 0,
                            //SteeringOverride = i * MathF.PI/3,
                            Position = new
                            {
                                X = 0 + (i * 150),
                                Y = 0
                            }
                        });

                CustomData = JsonConvert.SerializeObject(new
                {
                    Magic = JsonConvert.SerializeObject(new
                    {
                        IsShielded = true,
                        ShipSprite = 24,
                        Fleet = new
                        {
                            Burden = 1,
                            ShipConfig = ships
                        },
                    })
                });
            }
        }
    }
}
