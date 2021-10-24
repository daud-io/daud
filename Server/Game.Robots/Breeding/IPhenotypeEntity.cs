namespace Game.Robots.Breeding
{
    using System.Collections.Generic;

    public interface IPhenotypeEntity
    {
        IPhenotype[] Phenotypes { get; }
        void Load(IEnumerable<int> entityGenes);
    }


}