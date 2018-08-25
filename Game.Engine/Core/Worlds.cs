namespace Game.Engine.Core
{
    public static class Worlds
    {
        private static readonly World Default = new World();

        public static World Find()
        {
            return Default;
        }
    }
}
