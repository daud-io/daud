namespace Game.Robots.Behaviors
{
    using System;

    public class ContextRing
    {
        public readonly float[] Weights;
        private readonly float StepSize;

        public ContextRing(int size)
        {
            Weights = new float[size]; // a weight for each slice
            StepSize = MathF.PI * 2 / size; // how many radians in each slice of the pie
        }

        public float Angle(int step) => StepSize * step;
    }
}
