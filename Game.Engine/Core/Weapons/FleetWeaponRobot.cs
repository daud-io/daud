namespace Game.Engine.Core.Weapons
{
    using Game.API.Common;

    public class FleetWeaponRobot : IFleetWeapon
    {
        public bool IsOffense => false;
        public bool IsDefense => true;

        public void FireFrom(Fleet fleet)
        {
            var bot = new Robot()
            {
                ShipSprite = Sprites.ship0,
                Name = $"🤖",
                Color = fleet.Color,
                ControlInput = new ControlInput()
            };

            bot.Init(fleet.World);

            bot.Spawn(bot.Name, bot.ShipSprite, bot.Color, "");
        }
    }
}
