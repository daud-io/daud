namespace Game.Robots.Behaviors
{
    public class ContextBehavior : IBehaviors
    {
        public ContextRing Behave(int steps)
        {
            var ring = new ContextRing(steps);

            for(var i=0; i<steps; i++)
                ring.Weights[i] = ScoreAngle(ring.Angle(i));

            return ring;
        }

        protected virtual float ScoreAngle(float angle) => 0f;
    }
}
