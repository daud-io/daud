namespace Game.Robots.Senses
{
    using Game.API.Client;
    using Game.API.Common;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class SensorBullets : ISense
    {
        private readonly ContextRobot Robot;

        public IEnumerable<Body> VisibleBullets { get; private set; }

        public SensorBullets(ContextRobot robot)
        {
            this.Robot = robot;
            this.VisibleBullets = null;
        }

        public void Sense()
        {
            VisibleBullets = Robot.Bodies
                .Where(b => b.Group != null)
                .Where(b =>
                    b.Group.Type == GroupTypes.VolleyBullet
                    || b.Group.Type == GroupTypes.VolleySeeker)
                .Where(b => b.Group?.Owner != Robot.FleetID) // make sure it's not owned by this robot
                .OrderBy(b => Vector2.Distance(b.Position, Robot.Position)) // order them by range
                .ToList();
        }
    }
}
