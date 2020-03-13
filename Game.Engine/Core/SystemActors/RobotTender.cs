namespace Game.Engine.Core.SystemActors
{
    using Game.API.Common;
    using System;
    using System.Collections.Generic;

    public class RobotTender : IActor
    {
        private readonly List<Robot> Robots = new List<Robot>();
        private World World = null; 

        Random rand = new Random();

        private void AddRobot()
        {
            string color = "";
            Sprites sprite = new Sprites();

            //Random r = new Random();
            int rInt = rand.Next(7); //r.Next(0, 6);
            switch (rInt)
            {
                case 0:
                    color = "red";
                    sprite = Sprites.ship_red;
                    break;
                case 1:
                    color = "orange";
                    sprite = Sprites.ship_orange;
                    break;
                case 2:
                    color = "yellow";
                    sprite = Sprites.ship_yellow;
                    break;
                case 3:
                    color = "green";
                    sprite = Sprites.ship_green;
                    break;
                case 4:
                    color = "cyan";
                    sprite = Sprites.ship_cyan;
                    break;
                case 5:
                    color = "blue";
                    sprite = Sprites.ship_blue;
                    break;
                default:
                    color = "pink";
                    sprite = Sprites.ship_pink;
                    break;
            }

            // Console.WriteLine(rInt + " " + color);

            var bot = new Robot()
            {
                ShipSprite = sprite,
                Name = $"🤖Daudelin #{this.Robots.Count}",
                ControlInput = new ControlInput()
            };

            bot.Init(World);

            bot.Spawn(bot.Name, bot.ShipSprite, color, "");

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