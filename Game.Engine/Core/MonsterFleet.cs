namespace Game.Engine.Core
{
    using Newtonsoft.Json;
    public class MonsterFleet : Fleet
    {

        public override void Think()
        {
            DoShipConfig();

            base.Think();
        }

        public string[] ShipConfig = null;
        private void DoShipConfig()
        {
            if (ShipConfig != null && Ships != null)
                for(int i = 0; i<ShipConfig.Length; i++)
                    if (Ships.Count > i)
                        JsonConvert.PopulateObject(ShipConfig[i], Ships[i]);
        }
    }
}
