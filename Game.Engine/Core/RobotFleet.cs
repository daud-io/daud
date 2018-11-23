namespace Game.Engine.Core
{
    public class RobotFleet : Fleet
    {

        public override float ShotCooldownTimeM { get => World.Hook.ShotCooldownTimeBotM; }
        public override float ShotCooldownTimeB { get => World.Hook.ShotCooldownTimeBotB * 2; }
        public override float BaseThrustM { get => World.Hook.BaseThrustM; }
        public override float BaseThrustB { get => World.Hook.BaseThrustB; }


    }
}