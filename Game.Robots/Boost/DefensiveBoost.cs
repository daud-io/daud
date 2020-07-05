namespace Game.Robots.Boost
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class DefensiveBoost : BoostBehavior
    {
        public bool Enabled { get; set; } = false;

        public DefensiveBoost(ConfigTurret robot): base(robot)
        {
        }

        public override void Behave()
        {
            if (!Enabled)
                return;

            var myCenter = Robot.SensorFleets?.MyFleet?.Center;

            if (myCenter == null)
                return;
            
            var bullets = Robot.SensorBullets.VisibleBullets.Where(b => b.Group.Owner != Robot.FleetID)
                .OrderBy(p => Vector2.Distance(p.Position, Robot.SensorFleets.MyFleet.Center))
                .ToList();

            if (bullets.Any())
            {
                var bullet = bullets.First();

                var distance = Vector2.Distance(bullet.Position, myCenter.Value);
                if (distance < 200)
                    Robot.Boost();
            }
        }
    }
}
