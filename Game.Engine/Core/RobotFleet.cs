namespace Game.Engine.Core
{
    public class RobotFleet : Fleet
    {

        public override int ShootCooldownTime { get => World.Hook.ShootCooldownTimeBot; }
        public override float BaseThrust { get => World.Hook.BaseThrustBot; }
        public override float MaxSpeed { get => World.Hook.MaxSpeedBot; }
        public override float MaxSpeedBoost { get => World.Hook.MaxSpeedBoost; }

    }
}