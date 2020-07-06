namespace Game.Engine.Core
{
    using Newtonsoft.Json;
    using System;

    public class MonsterFleet : Fleet
    {

        public override void Think()
        {
            DoShipConfig();

            base.Think();
        }

        public string PlayerColor
        {
            set
            {
                this.Owner.Color = value;
            }
        }

        public string[] ShipConfig = null;
        private void DoShipConfig()
        {
            try
            {
                if (ShipConfig != null && Ships != null)
                    for (int i = 0; i < ShipConfig.Length; i++)
                        if (Ships.Count > i)
                            if (ShipConfig[i] != null && ShipConfig[i] != "null")
                                JsonConvert.PopulateObject(ShipConfig[i], Ships[i]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Bad monster config: " + e);
            }
        }
    }
}
