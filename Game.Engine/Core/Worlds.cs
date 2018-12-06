namespace Game.Engine.Core
{
    public static class Worlds
    {
        private static readonly World Default;
        private static readonly World Other;
        private static readonly World Duel;
        private static readonly World CTF;

        static Worlds()
        {
            Default = new World();
            Other = WorldOther();
            Duel = WorldDuel();
            CTF = WorldCTF();
        }

        private static World WorldOther()
        {
            var hook = Hook.Default;
            hook.BotBase = 10;
            return new World
            {
                Hook = hook
            };
        }

        private static World WorldDuel()
        {
            var hook = Hook.Default;
            hook.BotBase = 0;
            hook.WorldSize = 3500;
            hook.Obstacles = 0;
            hook.Fishes = 25;
            return new World
            {
                Hook = hook
            };
        }

        private static World WorldCTF()
        {
            var hook = Hook.Default;
            hook.BotBase = 0;
            hook.Obstacles = 0;
            hook.CTFMode = true;

            return new World
            {
                Hook = hook
            };
        }

        public static World Find(string world = null)
        {
            switch (world)
            {
                case "duel":
                    return Duel;
                case "other":
                    return Other;
                case "ctf":
                    return CTF;
                case "default":
                default:
                    return Default;
            }
        }
    }
}
