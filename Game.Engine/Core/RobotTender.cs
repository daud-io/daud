namespace Game.Engine.Core
{
    using Game.API.Common;
    using System.Collections.Generic;

    public class RobotTender : IActor
    {
        private readonly List<Robot> Robots = new List<Robot>();
        private World World = null;

        private void AddRobot()
        {
            var bot = new Robot()
            {
                ShipSprite = Sprites.ship0,
                Name = $"🤖Daudelin #{this.Robots.Count}",
                ControlInput = new ControlInput()
            };

            bot.Init(World);

            bot.Spawn(bot.Name, bot.ShipSprite, "green", "");

            this.Robots.Add(bot);
        }

        public void Think()
        {
        }

        public void Init(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);
        }

        public void CreateDestroy()
        {
            int desired = World.Hook.BotBase;

            while (Robots.Count < desired)
                AddRobot();

            while (Robots.Count > desired)
            {
                var robot = Robots[Robots.Count - 1];
                Robots.Remove(robot);
                robot.AutoSpawn = false;
                robot.Die();
                robot.Destroy();
            }
        }

        public void Destroy()
        {
            this.World.Actors.Remove(this);
        }
    }
}