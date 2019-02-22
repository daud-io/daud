namespace Game.Robots.Breeding
{
    using GeneticSharp.Domain.Chromosomes;

    public class RobotChromosome : BitStringChromosome<BehaviorPhenotypeEntity>
    {
        private RobotEvolutionConfiguration Config;
        public RobotChromosome(RobotEvolutionConfiguration config)
        {
            Config = config;

            var phenotypeEntities = new BehaviorPhenotypeEntity[config.BehaviorCount];

            for (int i = 0; i < phenotypeEntities.Length; i++)
            {
                phenotypeEntities[i] = new BehaviorPhenotypeEntity(config, i);
            }

            SetPhenotypes(phenotypeEntities);
            CreateGenes();
        }

        public override IChromosome CreateNew()
        {
            return new RobotChromosome(Config);
        }
    }
}
