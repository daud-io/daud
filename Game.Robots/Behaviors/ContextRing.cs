namespace Game.Robots.Behaviors
{
    using System;
    using System.Linq;

    public class ContextRing
    {
        public readonly float[] Weights;
        private readonly float StepSize;
        public float RingWeight { get; set; } = 1f;
        public string Name { get; set; }

        public ContextRing(ContextRing template)
        {
            this.Weights = template.Weights.ToArray();
            this.StepSize = template.StepSize;
            this.RingWeight = template.RingWeight;
            this.Name = template.Name;
        }

        public ContextRing(int size)
        {
            Weights = new float[size]; // a weight for each slice
            StepSize = MathF.PI * 2 / size; // how many radians in each slice of the pie
        }

        public float Angle(int step) => StepSize * step;

        public void Normalize()
        {
            var max = Weights.Max(w => w);
            var min = Weights.Min(w => w);
            if (max != 0 || min != 0)
            {
                float factor = 1f / (max - min);
                for (var i = 0; i < Weights.Length; i++)
                    Weights[i] = (Weights[i] - min) * factor;
            }
        }
    }
}
