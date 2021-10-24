namespace Game.Robots.Breeding
{
    public class BehaviorPhenotypeEntity : PhenotypeEntityBase
    {
        public const int LookAheadCyclesBits = 8;
        public const int BehaviorWeightBits = 6;
        public const int PhenotypeSize = LookAheadCyclesBits + BehaviorWeightBits;

        public BehaviorPhenotypeEntity(RobotEvolutionConfiguration config, int entityIndex)
        {

            Phenotypes = new IPhenotype[]
            {
                new Phenotype("LookAheadCycles", LookAheadCyclesBits)
                {
                    MinValue = 0,
                    MaxValue = 254
                },
                new Phenotype("BehaviorWeight", BehaviorWeightBits)
                {
                    MinValue = 0,
                    MaxValue = 10
                }
            };
        }

        public int LookAheadMS
        {
            get
            {
                return (int)Phenotypes[0].Value * 40;
            }
        }

        public float BehaviorWeight
        {
            get
            {
                return (float)Phenotypes[1].Value;
            }
        }
    }
}
