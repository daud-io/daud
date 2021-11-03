namespace Game.Robots.Breeding
{
    public interface IPhenotype
    {
        string Name { get; }
        int Length { get; }
        double MinValue { get; }
        double MaxValue { get; }
        double Value { get; set; }

        double RandomValue();
    }


}