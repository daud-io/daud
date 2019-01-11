namespace Game.Robots.Behaviors
{
    public interface IBehaviors
    {
        void Reset();
        ContextRing Behave(int steps);
    }
}
