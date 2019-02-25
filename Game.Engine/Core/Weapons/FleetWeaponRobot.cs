namespace Game.Engine.Core.Weapons
{
    using Game.API.Common;
    using System.Linq;
    using System.Numerics;

    public class FleetWeaponRobot : IFleetWeapon
    {
        public bool IsOffense => true;
        public bool IsDefense => false;

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
            bot.OneLifeOnly = true;
            bot.SpawnLocation = fleet.FleetCenter;
            bot.ExcludeFleet = fleet;

            if (fleet.AimTarget != Vector2.Zero)
                bot.SpawnMomentum = Vector2.Normalize(fleet.AimTarget)
                    * ((fleet.Ships.Count() * fleet.ShotThrustM + fleet.ShotThrustB) * 10);

            bot.Spawn(bot.Name, bot.ShipSprite, bot.Color, "");

            bot.SetInvulnerability(0);
        }
    }
}
