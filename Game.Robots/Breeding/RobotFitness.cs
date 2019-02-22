namespace Game.Robots.Breeding
{
    using GeneticSharp.Domain.Chromosomes;
    using GeneticSharp.Domain.Fitnesses;
    using System;
    using System.IO;
    using System.Threading;
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

            ConfigurableContextBot robot = null;

            Task.Run(async () =>
            {
                robot = await this.BotFactoryAsync(c);

                var cts = new CancellationTokenSource();
                cts.CancelAfter(5000);

                try
                {
                    await robot.StartAsync(cts.Token);
                }
                catch (TaskCanceledException) { }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Console.WriteLine("exception in RobotFitness: " + e);
                }
            }).Wait();

            return robot.StatsDeaths > 0
                ? robot.StatsKills / robot.StatsDeaths
                : double.MaxValue;
        }
    }
}