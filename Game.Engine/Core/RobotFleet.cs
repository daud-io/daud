namespace Game.Engine.Core
{
    public class RobotFleet : Fleet
    {
        public override float ShotCooldownTimeM { get => World.Hook.ShotCooldownTimeBotM; }
        public override float ShotCooldownTimeB { get => World.Hook.ShotCooldownTimeBotB; }
        public override float BaseThrustA { get => World.Hook.BaseThrustA; }
        public override float BaseThrustB { get => World.Hook.BaseThrustB; }
        public override float BaseThrustC { get => World.Hook.BaseThrustC; }
        public override int SpawnShipCount { get => World.Hook.BossMode ? 20 : World.Hook.SpawnShipCount; }
    }
}