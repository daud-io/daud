namespace Game.Robots.Behaviors
{
    public interface IBehavior
    {
        void Reset();
        ContextRing Behave(int steps);
    }
}
