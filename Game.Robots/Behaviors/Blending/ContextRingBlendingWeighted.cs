namespace Game.Robots.Behaviors.Blending
{
    using System.Collections.Generic;
    using System.Linq;

    public class ContextRingBlendingWeighted : IContextRingBlending
    {
        private readonly ContextRobot Robot;
        public int BlurSteps { get; set; } = 10;
        public float BlurAmount { get; set; } = 0.05f;
        public int BlurResolutionMultiplier { get; set; } = 1;

        public ContextRingBlendingWeighted(ContextRobot robot)
        {
            this.Robot = robot;
        }

        public (ContextRing, float,bool) Blend(IEnumerable<ContextRing> contexts,bool doBoost)
        {
            var finalSteps = Robot.Steps * BlurResolutionMultiplier;
            var combined = new ContextRing(finalSteps);

            if (BlurResolutionMultiplier > 1)
                contexts = contexts.Select(c => c.ResolutionMultiply(BlurResolutionMultiplier)).ToList();

            if (contexts.Any())
            {
                // blur
                foreach (var context in contexts)
                    BlurRing(context);

                for (var i = 0; i < finalSteps; i++)
                    combined.Weights[i] = contexts.Sum(c => c.Weights[i] * c.RingWeight);
                for (var i = 0; i < finalSteps; i++)
                    combined.WeightsBoost[i] = contexts.Sum(c => c.WeightsBoost[i] * c.RingWeight);

                var maxIndex = 0;

                for (var i = 0; i < finalSteps; i++)
                {
                    if (combined.Weights[i] > combined.Weights[maxIndex])
                        maxIndex = i;
                }
                var maxBoostIndex = 0;

                for (var i = 0; i < finalSteps; i++)
                {
                    if (combined.WeightsBoost[i] > combined.WeightsBoost[maxBoostIndex])
                        maxBoostIndex = i;
                }
                bool willBoost=false;
                var bestIndex=maxIndex;
                if(combined.Weights[maxIndex]<combined.WeightsBoost[maxBoostIndex]-0.9 && doBoost){
                    willBoost=true;
                    bestIndex=maxBoostIndex;
                }

                return (combined, combined.Angle(bestIndex),willBoost);
            }
            else
            {
                return (null, 0,false); // going east a lot ?
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
