namespace Game.Robots.Breeding
{
    using Game.Robots.Contests;
    using GeneticSharp.Domain;
    using GeneticSharp.Domain.Crossovers;
    using GeneticSharp.Domain.Mutations;
    using GeneticSharp.Domain.Populations;
    using GeneticSharp.Domain.Selections;
    using GeneticSharp.Infrastructure.Framework.Threading;
    using System;
    using System.Threading.Tasks;

    public class RobotEvolutionController
    {
        private int NumberOfSimultaneousEvaluations = 1;

        public GeneticAlgorithm CreateGA(Func<RobotChromosome, Task<ContestGame>> contestFactory, RobotEvolutionConfiguration config)
        {
            NumberOfSimultaneousEvaluations = 2;
            var fitness = new RobotFitness(contestFactory, config);
            var chromosome = new RobotChromosome(config);
            var crossover = new UniformCrossover();
            var mutation = new FlipBitMutation();
            var selection = new EliteSelection();
            var population = new Population(NumberOfSimultaneousEvaluations, NumberOfSimultaneousEvaluations, chromosome)
            {
                GenerationStrategy = new PerformanceGenerationStrategy()
            };

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new RobotTermination(),

                //TaskExecutor = new LinearTaskExecutor(),
                TaskExecutor = new ParallelTaskExecutor
                {
                    MaxThreads = 10
                }
            };
            ga.GenerationRan += delegate
            {
                Console.WriteLine("Generation complete");
            };

            ga.MutationProbability = .1f;

            return ga;
        }
    }
}