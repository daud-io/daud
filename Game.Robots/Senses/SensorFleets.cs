namespace Game.Robots.Senses
{
    using Game.API.Common;
    using Game.Robots.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SensorFleets : ISense
    {
        private readonly Robot Robot;

        public IEnumerable<Fleet> VisibleFleets { get; private set; }

        public SensorFleets(Robot robot)
        {
            this.Robot = robot;
            this.VisibleFleets = null;
        }

        public void Sense()
        {
            var shipSprites = new[] {
                Sprites.ship0,
                Sprites.ship_cyan,
                Sprites.ship_green,
                Sprites.ship_orange,
                Sprites.ship_pink,
                Sprites.ship_red,
                Sprites.ship_yellow,
                Sprites.ship_flash,
                Sprites.ship_secret,
                Sprites.ship_zed
            };

            VisibleFleets = Robot.Bodies
                .Where(b => shipSprites.Contains(b.Sprite)) // check the sprite
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
        }
    }
}
