namespace Game.Engine.Core.Actors.Bots
{
    using System.Collections.Generic;
    using System.Linq;

    public class RobotTender : IActor
    {
        private readonly List<Robot> Robots = new List<Robot>();
        private World World = null;

        private void AddRobot()
        {
            var bot = new Robot()
            {
            };
            bot.Init(World);

            bot.Spawn();
            bot.Fleet.Caption = $"Daudelin #{this.Robots.Count}";
            bot.Fleet.Sprite = "ship0";

            this.Robots.Add(bot);
        }

        public void Step()
        {
            int desired = 10;

            while (Robots.Count < desired)
                AddRobot();

            while (Robots.Count > desired)
            {
                var robot = Robots[Robots.Count - 1];
                Robots.Remove(robot);
                robot.Die();
                robot.Deinit();
            }
        }

        public void Init(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);
        }

        public void Deinit()
        {
            this.World.Actors.Remove(this);
        }
    }
}