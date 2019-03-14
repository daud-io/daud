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
            var robotName = "⚙" + fleet.Owner?.Name;

            var bot = new Robot()
            {
                ShipSprite = Sprites.ship0,
                Name = robotName,
                Color = fleet.Color,
                ControlInput = new ControlInput()
            };

            bot.Init(fleet.World);
            bot.OneLifeOnly = true;
            bot.SpawnLocation = fleet.FleetCenter;
            bot.ExcludeFleet = fleet;
            bot.AttackRobots = true;
            bot.ShipSize = 40;
            bot.TimeDeath = fleet.World.Time + 10000;
            bot.ShipSprite = fleet.Owner?.ShipSprite ?? Sprites.ship0;

            if (fleet.AimTarget != Vector2.Zero)
                bot.SpawnMomentum = Vector2.Normalize(fleet.AimTarget)
                    * ((fleet.Ships.Count() * fleet.Ships.Count() * fleet.ShotThrustA + fleet.Ships.Count() * fleet.ShotThrustM + fleet.ShotThrustB) * 20);

            bot.Spawn(bot.Name, bot.ShipSprite, bot.Color, "");
        }
    }
}
