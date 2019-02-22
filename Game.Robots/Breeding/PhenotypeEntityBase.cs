namespace Game.Robots.Breeding
{
    using System;
    using System.Linq;
    using GeneticSharp.Infrastructure.Framework.Commons;
    using System.Collections.Generic;

    public abstract class PhenotypeEntityBase : IPhenotypeEntity
    {
        public IPhenotype[] Phenotypes { get; protected set; }

        public void Load(IEnumerable<int> entityGenes)
        {
            var skip = 0;

            foreach (var p in Phenotypes)
            {
                p.Value = GetValue(entityGenes, skip, p);
                skip += p.Length;
            }
        }

        private double GetValue(IEnumerable<int> genes, int skip, IPhenotype phenotype)
        {
            var representation = string.Join(String.Empty, genes.Skip(skip).Take(phenotype.Length));
            var value = (float)BinaryStringRepresentation.ToDouble(representation, 0);

            if (value < phenotype.MinValue)
                return phenotype.MinValue;

            if (value > phenotype.MaxValue)
                return phenotype.MaxValue;

            return value;
        }
    }
}