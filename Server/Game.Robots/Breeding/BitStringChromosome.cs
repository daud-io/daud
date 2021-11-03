namespace Game.Robots.Breeding
{
    using GeneticSharp.Domain.Chromosomes;
    using GeneticSharp.Infrastructure.Framework.Commons;
    using System;
    using System.Linq;

    public abstract class BitStringChromosome<TPhenotypeEntity> : BinaryChromosomeBase
        where TPhenotypeEntity : IPhenotypeEntity
    {
        private TPhenotypeEntity[] m_phenotypeEntities;
        private string m_originalValueStringRepresentation;

        protected BitStringChromosome()
            : base(2)
        {
        }

        protected void SetPhenotypes(params TPhenotypeEntity[] phenotypeEntities)
        {
            if (phenotypeEntities.Length == 0)
            {
                throw new ArgumentException("At least one phenotype entity should be informed.", nameof(phenotypeEntities));
            }

            m_phenotypeEntities = phenotypeEntities;
            Resize(m_phenotypeEntities.Sum(e => e.Phenotypes.Sum(p => p.Length)));
        }

        public virtual TPhenotypeEntity[] GetPhenotypes()
        {
            var genes = GetGenes();
            var skip = 0;
            var entityLength = 0;

            foreach (var entity in m_phenotypeEntities)
            {
                entityLength = entity.GetTotalBits();
                entity.Load(genes.Skip(skip).Take(entityLength).Select(g => (int)g.Value));
                skip += entityLength;
            }

            return m_phenotypeEntities;
        }

        protected override void CreateGenes()
        {
            var valuesLength = m_phenotypeEntities.Sum(p => p.Phenotypes.Length);
            var originalValues = new double[valuesLength];
            var totalBits = new int[valuesLength];
            var fractionBits = new int[valuesLength];
            IPhenotype phenotype;

            int valueIndex = 0;
            foreach (var entity in m_phenotypeEntities)
            {
                for (int i = 0; i < entity.Phenotypes.Length; i++)
                {
                    phenotype = entity.Phenotypes[i];
                    originalValues[valueIndex] = phenotype.RandomValue();
                    totalBits[valueIndex] = phenotype.Length;
                    fractionBits[valueIndex] = 0;

                    valueIndex++;
                }
            }

            m_originalValueStringRepresentation = String.Join(
                String.Empty,
                BinaryStringRepresentation.ToRepresentation(
                    originalValues,
                    totalBits,
                    fractionBits));

            base.CreateGenes();
        }

        public override Gene GenerateGene(int geneIndex)
        {
            return new Gene(Convert.ToInt32(m_originalValueStringRepresentation[geneIndex].ToString()));
        }
    }
}