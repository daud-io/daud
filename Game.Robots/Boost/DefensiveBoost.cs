namespace Game.Robots.Boost
{
    using System.Linq;
    using System.Numerics;

    // this is basically Daudelin#0's panic boost.
    // strategy: bullet's are icky, and if one looks like it might be about
    //      to touch you, jam the boost button and hope you're facing a good direction
    public class DefensiveBoost : BoostBehavior
    {
        public bool Enabled { get; set; } = false;

        public DefensiveBoost(ConfigTurret robot) : base(robot)
        {
        }

        public override void Behave()
        {
            if (!Enabled)
                return;

            var myCenter = Robot.SensorFleets?.MyFleet?.Center;

            if (myCenter == null)
                return;

            if (Robot.SensorBullets.VisibleBullets.Where(b => b.Group.Owner != Robot.FleetID)
                .Any(b => Vector2.Distance(b.Position, myCenter.Value) < 200))
            {
                Robot.Boost();
            }
        }
    }
}
