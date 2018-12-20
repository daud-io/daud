namespace Game.Robots.Behaviors
{
    public class ContextBehavior : IBehaviors
    {
        public virtual float BehaviorWeight { get; set; } = 1f;

        public ContextRing Behave(int steps)
        {
            var ring = new ContextRing(steps);
            this.PreSweep();

            for (var i = 0; i < steps; i++)
                ring.Weights[i] = ScoreAngle(ring.Angle(i));

            ring.RingWeight = BehaviorWeight;
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
