namespace Game.Robots.Behaviors
{
    using System.Collections.Generic;
    using System.Linq;

    public class ContextRingBlendingWeighted : IContextRingBlending
    {
        private readonly ContextRobot Robot;
        public int BlurSteps { get; set; } = 10;
        public float BlurAmount { get; set; } = 0.05f;

        public ContextRingBlendingWeighted(ContextRobot robot)
        {
            this.Robot = robot;
        }

        public float Blend(IEnumerable<ContextRing> contexts)
        {
            var combined = new ContextRing(Robot.Steps);

            // blur
            foreach (var context in contexts)
                BlurRing(context);

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
            {
                return 0; // going east a lot ?
            }
        }

        private void BlurRing(ContextRing ring)
        {
            // blur
            for (var blurStep = 0; blurStep < BlurSteps; blurStep++)
                for (var i = 0; i < ring.Weights.Length; i++)
                {
                    var previousIndex = i - 1;
                    if (previousIndex < 0)
                        previousIndex = ring.Weights.Length - 1;

                    var prev = ring.Weights[previousIndex];
                    var next = ring.Weights[(i + 1) % ring.Weights.Length];

                    var thisScore = ring.Weights[i];
                    ring.Weights[i] +=
                        (prev - thisScore) * BlurAmount
                        + (next - thisScore) * BlurAmount;
                }
        }
    }
}
