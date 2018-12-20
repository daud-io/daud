namespace Game.Robots.Behaviors
{
    using System.Collections.Generic;
    using System.Linq;

    public class ContextRingBlendingWeighted
    {
        private readonly ContextRobot Robot;

        public ContextRingBlendingWeighted(ContextRobot robot)
        {
            this.Robot = robot;
        }

        public float Blend(IEnumerable<ContextRing> contexts)
        {
            var combined = new ContextRing(Robot.Steps);

            if (contexts.Any())
            {
                for (var i = 0; i < Robot.Steps; i++)
                    combined.Weights[i] = contexts.Sum(c => c.Weights[i]);

                var maxIndex = 0;

                for (var i = 0; i < Robot.Steps; i++)
                {
                    if (combined.Weights[i] > combined.Weights[maxIndex])
                        maxIndex = i;
                }

                return combined.Angle(maxIndex);
            }
            else
                return 0; // going east a lot ?
        }
    }
}
