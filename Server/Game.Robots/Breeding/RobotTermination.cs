namespace Game.Robots.Breeding
{
    using GeneticSharp.Domain;
    using GeneticSharp.Domain.Terminations;
    using System;

    public class RobotTermination : TerminationBase
    {
        // private double m_lastFitness;
        // private int m_stagnantGenerationsCount;

        protected override bool PerformHasReached(IGeneticAlgorithm geneticAlgorithm)
        {
            /*var ga = geneticAlgorithm as GeneticAlgorithm;

            var bestFitness = geneticAlgorithm.BestChromosome.Fitness.Value;

            if (bestFitness <= m_lastFitness)
            {
                m_stagnantGenerationsCount++;
                Console.WriteLine($"Fitness stagnant for {m_stagnantGenerationsCount} generations.");
            }
            else
            {
                m_stagnantGenerationsCount = 1;
            }

            m_lastFitness = bestFitness;

      
            foreach (var c in ga.Population.CurrentGeneration.Chromosomes)
            {
                c.Fitness = null;
            }

            return m_stagnantGenerationsCount >= 50;*/

            return false;
        }
    }
}