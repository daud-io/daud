namespace Game.Engine.Core
{
    public static class Worlds
    {
        private static readonly World Default;
        private static readonly World Other;
        private static readonly World Duel;
        private static readonly World CTF;
        private static readonly World Team;

        static Worlds()
        {
            Default = new World();
            Other = WorldOther();
            Duel = WorldDuel();
            Team = WorldTeam();
            CTF = WorldCTF();
        }

        private static World WorldOther()
        {
            var hook = Hook.Default;
            hook.BotBase = 10;
            hook.BotRespawnDelay = 0;
            return new World
            {
                Hook = hook
            };
        }

        private static World WorldDuel()
        {
            var hook = Hook.Default;
            hook.BotBase = 0;
            hook.WorldSize = 4200;
            hook.Obstacles = 3;
            hook.Fishes = 7;
            hook.Pickups = 3;
            hook.PointsPerKillFleet = 1;
            hook.PointsPerKillShip = 0;
            hook.PointsPerUniverseDeath = -1;
            hook.PointsMultiplierDeath = 1.0f;

            return new World
            {
                Hook = hook
            };
        }

        private static World WorldTeam()
        {
            var hook = Hook.Default;
            hook.BotBase = 0;
            hook.Obstacles = 3;
            hook.PointsPerKillFleet = 1;
            hook.PointsPerKillShip = 0;
            hook.PointsPerUniverseDeath = -1;
            hook.PointsMultiplierDeath = 1.0f;
            hook.TeamMode = true;

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
                case "team":
                    return Team;
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
