namespace Game.Engine.Core
{
    public class RobotFleet : Fleet
    {
        public override float ShotCooldownTimeM { get => World.Hook.ShotCooldownTimeBotM; }
        public override float ShotCooldownTimeB { get => World.Hook.ShotCooldownTimeBotB; }
        public override float BaseThrustM { get => World.Hook.BaseThrustM; }
        public override float BaseThrustB { get => World.Hook.BaseThrustB; }
        public override int SpawnShipCount { get => World.Hook.BossMode ? 20 : World.Hook.SpawnShipCount; }
    }
}