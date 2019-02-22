namespace Game.Robots.Breeding
{
    using GeneticSharp.Domain.Chromosomes;
    using GeneticSharp.Domain.Fitnesses;
    using System;
    using System.Threading.Tasks;

    public class RobotFitness : IFitness
    {
        private readonly Func<RobotChromosome, Task<ConfigurableContextBot>> BotFactoryAsync;
        private readonly RobotEvolutionConfiguration RobotEvolutionConfiguration;
        public RobotFitness(Func<RobotChromosome, Task<ConfigurableContextBot>> botFactoryAsync,
            RobotEvolutionConfiguration config)
        {
            this.BotFactoryAsync = botFactoryAsync;
            this.RobotEvolutionConfiguration = config;
        }

        public double Evaluate(IChromosome chromosome)
        {
            var c = chromosome as RobotChromosome;

            Task.Run(async () =>
            {
                var robot = await this.BotFactoryAsync(c);
                await robot.StartAsync();
            }).Wait();

            return c.Score();
        }
    }
}