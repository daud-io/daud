namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using System.Numerics;
    using System.Threading.Tasks;

    public class ContextTurret : ContextRobot
    {
        protected readonly NavigateToPoint Navigation;

        public ContextTurret(Vector2 target)
        {
            Behaviors.Add(Navigation = new NavigateToPoint(this));
            Navigation.TargetPoint = target;
            Steps = 16;
        }
    }
}
