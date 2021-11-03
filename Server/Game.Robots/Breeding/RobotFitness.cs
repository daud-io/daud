namespace Game.Robots.Breeding
{
    using Game.Robots.Contests;
    using GeneticSharp.Domain.Chromosomes;
    using GeneticSharp.Domain.Fitnesses;
    using System;
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
            double score = 1;

            /*

            Task.Run(async () =>
            {

                Console.WriteLine("Evaluating Chromosome");
                var contest = await this.ContestFactoryAsync(c);
                try
                { 
                    var phenotypes = c.GetPhenotypes();

                    for (var i = 0; i < contest.TestRobot.ContextBehaviors.Count; i++)
                    {
                        contest.TestRobot.ContextBehaviors[i].BehaviorWeight = phenotypes[i].BehaviorWeight;
                        contest.TestRobot.ContextBehaviors[i].LookAheadMS = phenotypes[i].LookAheadMS;
                    }
                    contest.ChallengeRobot.Name = "challenge";
                    Console.WriteLine($"name: {contest.TestRobot.Name} vs. {contest.ChallengeRobot.Name}");

                    contest.ChallengeRobot.DuelingProtocol = true;
                    contest.ChallengeRobot.RespawnFalloffMS = 2000;
                    contest.TestRobot.DuelingProtocol = true;
                    contest.TestRobot.RespawnFalloffMS = 2000;

                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(RobotEvolutionConfiguration.FitnessDuration);

                    try
                    {
                        await Task.WhenAny(contest.TestRobot.StartAsync(cts.Token),
                            contest.ChallengeRobot.StartAsync(cts.Token));
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
                catch (Exception)
                {

                }
                finally
                {
                    await contest.FinishedAsync();
                }

            }).Wait();
            */

            c.Fitness = score;

            return score;
        }
    }
}