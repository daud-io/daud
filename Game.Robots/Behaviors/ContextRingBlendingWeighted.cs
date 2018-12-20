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


            // blur
            foreach (var context in contexts)
                for (var blurStep = 0; blurStep < 10; blurStep++)
                    for (var i = 0; i < context.Weights.Length; i++)
                    {
                        var previousIndex = i - 1;
                        if (previousIndex < 0)
                            previousIndex = context.Weights.Length - 1;
                        var prev = context.Weights[previousIndex];
                        var next = context[(i + 1) % context.Weights.Length];

                        var thisScore = context[i].score;
                        context[i].score = context[i].score + (prev - thisScore) * .05 + (next - thisScore) * .05;
                    }



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
