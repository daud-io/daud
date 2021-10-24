namespace Game.Robots.Senses
{
    using Game.API.Common;
    using Game.Robots.Models;
    using System.Collections.Generic;
    using System.Linq;

    public class SensorAbandoned : ISense
    {
        private readonly ContextRobot Robot;

        public IEnumerable<Ship> AllVisibleAbandoned { get; private set; }

        public SensorAbandoned(ContextRobot robot)
        {
            this.Robot = robot;
            this.AllVisibleAbandoned = null;
        }

        public void Sense()
        {
            AllVisibleAbandoned = Robot.Bodies
                .Where(b => b.Sprite == Sprites.ship_gray)
                .Select(b => new Ship
                {
                    ID = b.ID,
                    Angle = b.Angle,
                    Momentum = b.Velocity,
                    Position = b.Position,
                    Size = b.Size
                }).ToList();
        }
    }
}
