namespace Game.Robots.Behaviors.Blending
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ContextRingBlendingWeightedStark : IContextRingBlending
    {
        private readonly ContextRobot Robot;
        public int BlurSteps { get; set; } = 0;
        public float BlurAmount { get; set; } = 0.05f;

        public ContextRingBlendingWeightedStark(ContextRobot robot)
        {
            this.Robot = robot;
        }

        public float Blend(IEnumerable<ContextRing> contexts)
        {
            var combined = new ContextRing(Robot.Steps);

            
            // blur
            foreach (var context in contexts)
                BlurRing(context);

            lock (typeof(ContextRingBlendingWeighted))
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("RingDump");
                foreach (var context in contexts)
                {
                    var name = context.Name;
                    while (name.Length < 20)
                        name += ' ';
                    Console.WriteLine($"{name}\t{string.Join(',', context.Weights.Select(w => (w * context.RingWeight).ToString("+0.0;-0.0")))}");
                }
            }

            if (contexts.Any())
            {
                for (var i = 0; i < Robot.Steps; i++)
                    combined.Weights[i] = contexts.Sum(c => c.Weights[i] * c.RingWeight);

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
            // blur the values in the ring
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
