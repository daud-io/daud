namespace Game.Robots.Behaviors
{
    public class NavigateToPoint : IBehaviors
    {
        private readonly Robot Robot;

        public NavigateToPoint(Robot robot)
        {
            this.Robot = robot;
        }

        public ContextRing Behave(int steps)
        {
            var ring = new ContextRing(steps);



            return ring;
        }
    }
}
