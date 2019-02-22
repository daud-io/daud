namespace Game.Robots.Breeding
{
    using System.Linq;

    public static class PhenotypeEntityExtensions
    {
        public static int GetTotalBits(this IPhenotypeEntity entity)
        {
            return entity.Phenotypes.Sum(p => p.Length);
        }
    }


}