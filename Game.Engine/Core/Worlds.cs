namespace Game.Engine.Core
{
    public static class Worlds
    {
        private static World Default = new World();

        public static World Find()
        {
            return Default;
        }
    }
}
