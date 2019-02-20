namespace Game.Robots.GeneticConfiguration
{
    using GeneticSharp.Domain;
    using GeneticSharp.Domain.Crossovers;
    using GeneticSharp.Domain.Mutations;
    using GeneticSharp.Domain.Populations;
    using GeneticSharp.Domain.Selections;
    using GeneticSharp.Domain.Terminations;
    using GeneticSharp.Infrastructure.Framework.Threading;

    public class RobotEvolutionController
    {
        private static RobotEvolutionConfiguration s_config;

        private int NumberOfSimultaneousEvaluations = 1;
     
        public RobotEvolutionConfiguration Config;
     
        private RobotFitness m_fitness;

        public static void SetConfig(RobotEvolutionConfiguration config)
        {
            s_config = config;
        }

		private void Awake()
		{
			if(s_config != null)
            {
                Config = s_config;
            }
		}

		protected GeneticAlgorithm CreateGA()
        {
            NumberOfSimultaneousEvaluations = 1;
            m_fitness = new RobotFitness();
            var chromosome = new RobotChromosome(Config);      
            var crossover = new UniformCrossover();
            var mutation = new FlipBitMutation();
            var selection = new EliteSelection();
            var population = new Population(NumberOfSimultaneousEvaluations, NumberOfSimultaneousEvaluations, chromosome)
            {
                GenerationStrategy = new PerformanceGenerationStrategy()
            };

            var ga = new GeneticAlgorithm(population, m_fitness, selection, crossover, mutation);
            ga.Termination = new RobotTermination();
            ga.TaskExecutor = new ParallelTaskExecutor
            {
                MinThreads = population.MinSize,
                MaxThreads = population.MaxSize * 2
            };
            ga.GenerationRan += delegate
            {
                //m_lastPosition = Vector3.zero;
                //m_evaluationPool.ReleaseAll();
            };

            ga.MutationProbability = .1f;

            return ga;
        }

        public void StartSample()
        {

            var ga = CreateGA();
            
            // sync start
            ga.Start();
        }

        protected void UpdateSample()
        {
            // end evaluation.
            while (m_fitness.ChromosomesToEndEvaluation.Count > 0)
            {
                RobotChromosome c;
                m_fitness.ChromosomesToEndEvaluation.TryTake(out c);
                c.Evaluated = true;
            }

               
            // in evaluation.
            while (m_fitness.ChromosomesToBeginEvaluation.Count > 0)
            {
                RobotChromosome c;
                m_fitness.ChromosomesToBeginEvaluation.TryTake(out c);
                c.Evaluated = false;


                // reset score
                //c.MaxDistance = 0;

                //robot.SetChromosome(c, Config);
            }
        }
    }
}