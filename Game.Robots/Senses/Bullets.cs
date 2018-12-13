namespace Game.Robots.Senses
{
    using Game.API.Client;
    using Game.API.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class SensorBullets : ISense
    {
        private readonly Robot Robot;

        public IEnumerable<Body> VisibleBullets { get; private set; }

        public SensorBullets(Robot robot)
        {
            this.Robot = robot;
            this.VisibleBullets = null;
        }

        public void Sense()
        {
            var bulletSprites = new[] {
                Sprites.bullet,
                Sprites.bullet_cyan,
                Sprites.bullet_green,
                Sprites.bullet_orange,
                Sprites.bullet_pink,
                Sprites.bullet_red,
                Sprites.bullet_yellow,
                Sprites.seeker
            };

            VisibleBullets = Robot.Bodies
                .Where(b => bulletSprites.Contains(b.Sprite)) // check the sprite
                .Where(b => b.Group?.Owner != Robot.FleetID) // make sure it's not owned by this robot
                .OrderBy(b => Vector2.Distance(b.Position, Robot.Position)) // order them by range
                .ToList();
        }
    }
}
