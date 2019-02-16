namespace Game.Robots.Behaviors.Blending
{
    using System;
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

        public (ContextRing, float) Blend(IEnumerable<ContextRing> contexts)
        {
            var combined = new ContextRing(Robot.Steps);


            if (false)
            lock (typeof(ContextRingBlendingWeighted))
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("RingDump");
                foreach (var context in contexts)
                {
                    var name = context.Name;
                    while (name.Length < 20)
                        name += ' ';
                    Console.WriteLine($"{name}\t{string.Join(',', context.Weights.Select(w => (w * context.RingWeight).ToString("+0.00;-0.00")))}");
                }
            }


            // blur
            foreach (var context in contexts)
                BlurRing(context);

            if (false)
            lock (typeof(ContextRingBlendingWeighted))
            {
//                Console.SetCursorPosition(0, 0);
                Console.WriteLine("RingDump post blur");
                foreach (var context in contexts)
                {
                    var name = context.Name;
                    while (name.Length < 20)
                        name += ' ';
                    Console.WriteLine($"{name}\t{string.Join(',', context.Weights.Select(w => (w * context.RingWeight).ToString("+0.00;-0.00")))}");
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

                return (combined, combined.Angle(maxIndex));
            }
            else
            {
                return (null, 0); // going east a lot ?
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
