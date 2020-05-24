namespace Game.Robots.Monsters
{
    using Newtonsoft.Json;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    public class ConfigMonster : ConfigurableContextBot
    {
        private object ShipTemplate = null;
        
        protected override Task AliveAsync()
        {
            if (SensorFleets.MyFleet?.Ships != null)
            {
                if (Vector2.Distance(Vector2.Zero, SensorFleets.MyFleet.Center) > 1000)
                {
                    ShipTemplate = new
                    {
                        Size = 100,
                        Sprite = 1,
                        Position = new
                        {
                            X = 0,
                            Y = 0
                        }
                    };
                }
            }

            if (ShipTemplate != null)
            {
                SetShipTemplate(ShipTemplate);
                ShipTemplate = null;
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
                    ships[i] = JsonConvert.SerializeObject(template);

                CustomData = JsonConvert.SerializeObject(new
                {
                    Magic = JsonConvert.SerializeObject(new
                    {
                        ShipSprite = 1,
                        Fleet = new
                        {
                            ShipConfig = ships
                        },
                    })
                });
            }
        }
    }
}
