namespace Game.Robots.Senses
{
    using Game.API.Common;
    using Game.Robots.Models;
    using System.Collections.Generic;
    using System.Linq;

    public class SensorFleets : ISense
    {
        private readonly ContextRobot Robot;

        public IEnumerable<Fleet> AllVisibleFleets { get; private set; }

        public Fleet MyFleet { get; private set; }

        public SensorFleets(ContextRobot robot)
        {
            this.Robot = robot;
            this.AllVisibleFleets = null;
        }

        public void Sense()
        {
            AllVisibleFleets = Robot.Bodies
                .Where(b => b.Group?.Type == GroupTypes.Fleet) // check the sprite
                .GroupBy(b => b.Group)
                .Select(g => new Fleet
                {
                    ID = g.Key.ID,
                    Name = g.Key.Caption,
                    Sprite = g.Select(b => b.Sprite).FirstOrDefault(),
                    Ships = g.Select(b => new Fleet.Ship
                    {
                        Angle = b.Angle,
                        Momentum = b.Momentum,
                        Position = b.Position,
                        Size = b.Size
                    }).ToList()
                })
                .ToList();

            MyFleet = AllVisibleFleets.FirstOrDefault(f => f.ID == Robot.FleetID);
        }

        public IEnumerable<Fleet> Others { get => AllVisibleFleets.Except(new[] { MyFleet }); }
    }
}
