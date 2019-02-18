namespace Game.Robots.Senses
{
    using Game.API.Common;
    using Game.Robots.Models;
    using System.Collections.Generic;
    using System.Linq;

    public class SensorFish : ISense
    {
        private readonly ContextRobot Robot;

        public IEnumerable<Ship> AllVisibleFish { get; private set; }

        public SensorFish(ContextRobot robot)
        {
            this.Robot = robot;
            this.AllVisibleFish = null;
        }

        public void Sense()
        {
            AllVisibleFish = Robot.Bodies
                .Where(b => b.Group?.Type == GroupTypes.Fish || b.Sprite == Sprites.fish)
                .Select(b => new Ship
                {
                    ID = b.ID,
                    Angle = b.Angle,
                    Momentum = b.Momentum,
                    Position = b.Position,
                    Size = b.Size
                }).ToList();
        }
    }
}
