namespace Game.Robots.Boost
{
    public class SixMinuteAbs : BoostBehavior
    {
        public int BoostThreshold { get; set; } = 16;

        public SixMinuteAbs(ConfigTurret robot): base(robot)
        {
        }

        public override void Behave()
        {
            if (Robot.CanBoost && (Robot.SensorFleets.MyFleet?.Ships.Count ?? 0) > BoostThreshold)
                Robot.Boost();
        }
    }
}
