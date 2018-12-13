namespace Game.Engine.Core
{
    using System.Collections.Generic;

    public static class Worlds
    {
        public static readonly Dictionary<string, World> AllWorlds = new Dictionary<string, World>();

        private static readonly World Default;

        static Worlds()
        {
            Default = new World();
            AllWorlds.Add("default", Default);
            AllWorlds.Add("other", WorldOther());
            AllWorlds.Add("duel", WorldDuel());
            AllWorlds.Add("team", WorldTeam());
            AllWorlds.Add("ctf", WorldCTF());
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
            hook.Obstacles = 7;
            hook.CTFMode = true;
            hook.PointsPerKillFleet = 1;
            hook.PointsPerKillShip = 0;
            hook.PointsPerUniverseDeath = -1;
            hook.PointsMultiplierDeath = 1.0f;

            return new World
            {
                Hook = hook
            };
        }

        public static World Find(string world = null)
        {
            if (world != null && AllWorlds.ContainsKey(world))
                return AllWorlds[world];
            else
                return Default;
        }
    }
}

