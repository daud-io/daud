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
                //if (Vector2.Distance(Vector2.Zero, SensorFleets.MyFleet.Center) > 200)
                //{
                    ShipTemplate = new
                    {
                        Size = 100,
                        Sprite = 24,
                        Health = 1000,
                        ShieldStrength = 1000,
                        Position = new
                        {
                            X = 0,
                            Y = 0
                        }
                    };
                //}
            }

            if (ShipTemplate != null)
            {
                SetShipTemplate(ShipTemplate);
                ShipTemplate = null;
            }
            else
                SetShipTemplate(null);


            ShootAt(new Vector2(0, 100));

            return base.AliveAsync();
        }

        private void SetShipTemplate(object template)
        {
            if (SensorFleets?.MyFleet?.Ships?.Count > 0)
            {
                var ships = new string[SensorFleets.MyFleet.Ships.Count];
                for (int i = 0; i < ships.Length; i++)
                    ships[i] = template != null
                        ? JsonConvert.SerializeObject(template)
                        : null;

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
