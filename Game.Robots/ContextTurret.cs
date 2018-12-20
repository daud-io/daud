namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using System.Numerics;

    public class ContextTurret : ContextRobot
    {
        protected readonly NavigateToPoint Navigation;

        public ContextTurret(Vector2 target)
        {
            Behaviors.Add(Navigation = new NavigateToPoint(this) { BehaviorWeight = 1f });
            Behaviors.Add(new Efficiency(this) { BehaviorWeight = 0.5f });
            Behaviors.Add(new Dodge(this) { BehaviorWeight = 1f });

            Navigation.TargetPoint = target;
            Steps = 16;
        }
    }
}
