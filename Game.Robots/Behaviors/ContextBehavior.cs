namespace Game.Robots.Behaviors
{
    public class ContextBehavior : IBehaviors
    {
        public ContextRing Behave(int steps)
        {
            var ring = new ContextRing(steps);
            this.PreSweep();

            for (var i = 0; i < steps; i++)
                ring.Weights[i] = ScoreAngle(ring.Angle(i));

            return ring;
        }

        public virtual void Reset()
        {
        }

        protected virtual void PreSweep()
        {
        }

        protected virtual float ScoreAngle(float angle) => 0f;
    }
}
