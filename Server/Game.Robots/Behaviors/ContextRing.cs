namespace Game.Robots.Behaviors
{
    using System;
    using System.Linq;

    public class ContextRing
    {
        public readonly float[] Weights;
        public readonly float[] WeightsBoost;
        public float StepSize;
        public float RingWeight { get; set; } = 1f;
        public string Name { get; set; }

        public ContextRing(ContextRing template)
        {
            this.Weights = template.Weights.ToArray();
            this.WeightsBoost = template.WeightsBoost.ToArray();
            this.StepSize = template.StepSize;
            this.RingWeight = template.RingWeight;
            this.Name = template.Name;
        }

        public ContextRing(int size)
        {
            Weights = new float[size]; // a weight for each slice
            WeightsBoost = new float[size]; // a weight for each slice
            StepSize = MathF.PI * 2 / size; // how many radians in each slice of the pie
        }

        public float Angle(int step) => StepSize * step;

        public void Normalize()
        {
            var max = Weights.Concat(WeightsBoost).Max(w => w);
            var min = Weights.Concat(WeightsBoost).Min(w => w);
            if (max != 0 || min != 0)
            {
                float factor = 1f / (max - min);
                for (var i = 0; i < Weights.Length; i++)
                    Weights[i] = (Weights[i] - min) * factor;
                for (var i = 0; i < WeightsBoost.Length; i++)
                    WeightsBoost[i] = (WeightsBoost[i] - min) * factor;
            }
        }

        public ContextRing ResolutionMultiply(int multiplier)
        {
            var ring = new ContextRing(this.Weights.Length * multiplier)
            {
                RingWeight = this.RingWeight,
                Name = this.Name,
                StepSize = this.StepSize / multiplier
            };

            for (var i = 0; i < Weights.Length; i++)
            {
                for (var j = 0; j < multiplier; j++)
                    ring.Weights[i * multiplier + j] = this.Weights[i];
            }
            for (var i = 0; i < WeightsBoost.Length; i++)
            {
                for (var j = 0; j < multiplier; j++)
                    ring.WeightsBoost[i * multiplier + j] = this.WeightsBoost[i];
            }

            return ring;
        }
    }
}
