namespace Game.Robots.Behaviors
{
    public class NavigateToPoint : ContextBehavior
    {
        private readonly Robot Robot;

        public NavigateToPoint(Robot robot)
        {
            this.Robot = robot;
        }

        protected override float ScoreAngle(float angle)
        {

            return 0;
        }
    }
}
