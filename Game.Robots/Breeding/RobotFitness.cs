namespace Game.Robots.Breeding
{
    using Game.Robots.Contests;
    using GeneticSharp.Domain.Chromosomes;
    using GeneticSharp.Domain.Fitnesses;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class RobotFitness : IFitness
    {
        private readonly Func<RobotChromosome, Task<ContestGame>> ContestFactoryAsync;
        private readonly RobotEvolutionConfiguration RobotEvolutionConfiguration;
        public RobotFitness(Func<RobotChromosome, Task<ContestGame>> contestFactoryAsync,
            RobotEvolutionConfiguration config)
        {
            this.ContestFactoryAsync = contestFactoryAsync;
            this.RobotEvolutionConfiguration = config;
        }

        public double Evaluate(IChromosome chromosome)
        {
            var c = chromosome as RobotChromosome;
            double score = 0;

            Task.Run(async () =>
            {
                Console.WriteLine("Evaluating Chromosome");
                using (var contest = await this.ContestFactoryAsync(c))
                {
                    var phenotypes = c.GetPhenotypes();

                    for (var i = 0; i < contest.TestRobot.ContextBehaviors.Count; i++)
                    {
                        contest.TestRobot.ContextBehaviors[i].BehaviorWeight = phenotypes[i].BehaviorWeight;
                        contest.TestRobot.ContextBehaviors[i].LookAheadMS = phenotypes[i].LookAheadMS;
                    }

                    Console.WriteLine($"name: {contest.TestRobot.Name}");

                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(RobotEvolutionConfiguration.FitnessDuration);

                    try
                    {
                        await Task.WhenAny(
                            contest.TestRobot.StartAsync(cts.Token),
                            contest.ChallengeRobot.StartAsync(cts.Token)
                        );
                    }
                    catch (TaskCanceledException) { }
                    catch (OperationCanceledException) { }
                    catch (Exception e)
                    {
                        Console.WriteLine("exception in RobotFitness: " + e);
                    }

                    score = contest.TestRobot.StatsDeaths > 0
                        ? contest.TestRobot.StatsKills / contest.TestRobot.StatsDeaths
                        : double.MaxValue;

                    Console.WriteLine("Test Complete");
                    Console.WriteLine($"kills:{contest.TestRobot.StatsKills}\tdeaths:{contest.TestRobot.StatsDeaths}");
                    Console.WriteLine($"score:{score}");
                }

            }).Wait();

            return score;
        }
    }
}