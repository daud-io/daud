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
                Console.WriteLine("Evaluating Chromosome");
                robot = await this.BotFactoryAsync(c);

                Console.WriteLine($"name: {robot.Name}");

                var cts = new CancellationTokenSource();
                cts.CancelAfter(RobotEvolutionConfiguration.FitnessDuration);

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

                Console.WriteLine("Test Complete");
                Console.WriteLine($"kills:{robot.StatsKills}\tdeaths:{robot.StatsDeaths}");

            }).Wait();

            //c.Fitness

            return robot.StatsDeaths > 0
                ? robot.StatsKills / robot.StatsDeaths
                : double.MaxValue;
        }
    }
}