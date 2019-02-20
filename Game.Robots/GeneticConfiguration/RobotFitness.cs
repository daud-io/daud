namespace Game.Robots.GeneticConfiguration
{
    using GeneticSharp.Domain.Chromosomes;
    using GeneticSharp.Domain.Fitnesses;
    using System.Collections.Concurrent;
    using System.Threading;

    public class RobotFitness : IFitness
    {
        public RobotFitness()
        {
            ChromosomesToBeginEvaluation = new BlockingCollection<RobotChromosome>();
            ChromosomesToEndEvaluation = new BlockingCollection<RobotChromosome>();
        }

        public BlockingCollection<RobotChromosome> ChromosomesToBeginEvaluation { get; private set; }
        public BlockingCollection<RobotChromosome> ChromosomesToEndEvaluation { get; private set; }
        public double Evaluate(IChromosome chromosome)
        {
            var c = chromosome as RobotChromosome;
            ChromosomesToBeginEvaluation.Add(c);

            do
            {
                Thread.Sleep(1000);
                c.Fitness = c.Score();
            } while (!c.Evaluated);

            ChromosomesToEndEvaluation.Add(c);

            do
            {
                Thread.Sleep(100);
            } while (!c.Evaluated);

            return c.Score();
        }
    }
}