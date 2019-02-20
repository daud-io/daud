namespace Game.Robots.GeneticConfiguration
{
    using GeneticSharp.Domain.Chromosomes;

    public class RobotChromosome : BitStringChromosome<BehaviorPhenotypeEntity>
    {
        private RobotEvolutionConfiguration Config;
        public RobotChromosome(RobotEvolutionConfiguration config)
        {
            Config = config;

            var phenotypeEntities = new BehaviorPhenotypeEntity[config.BehaviorCount];

            for (int i = 0; i < phenotypeEntities.Length; i ++)
            {
                phenotypeEntities[i] = new BehaviorPhenotypeEntity(config, i);
            }

            SetPhenotypes(phenotypeEntities);
            CreateGenes();
        }

        public string ID { get; } = System.Guid.NewGuid().ToString();

        public bool Evaluated { get; set; }
        public float MaxDistance { get; set; }
        public float MaxDistanceTime { get; set; }
        public float MaxVelocity 
        { 
            get 
            {
                return MaxDistanceTime > 0 ? MaxDistance / MaxDistanceTime : 0; 
                            
            } 
        }
      
        public override IChromosome CreateNew()
        {
            return new RobotChromosome(Config);
        }
    }
}
