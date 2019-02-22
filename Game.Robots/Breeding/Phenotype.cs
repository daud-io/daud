namespace Game.Robots.Breeding
{
    using GeneticSharp.Domain.Randomizations;
    using System.Diagnostics;

    [DebuggerDisplay("{Name} = {MinValue} <= {Value} <= {MaxValue}")]
    public class Phenotype : IPhenotype
    {
        public Phenotype(string name, int length)
        {
            Name = name;
            Length = length;
        }

        public string Name { get; }
        public int Length { get; }
        public double MinValue { get; set; } = 0;
        public double MaxValue { get; set; } = 100;
        public virtual double Value { get; set; }

        public virtual double RandomValue()
        {
            return RandomizationProvider.Current.GetDouble(MinValue, MaxValue + 1);
        }
    }
}