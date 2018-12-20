namespace Game.Engine.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Worlds
    {
        public static readonly Dictionary<string, World> AllWorlds = new Dictionary<string, World>();

        private static readonly World Default;

        private static readonly string[] AllColors = new[] {
            "green",
            "orange",
            "pink",
            "red",
            "cyan",
            "yellow"
        };
        private static readonly string[] TeamColors = new[] {
            "red",
            "cyan"
        };

        static Worlds()
        {
            Default = new World()
            {
                Name = "Default",
                Description = "FFA Arena",
                AllowedColors = AllColors
            };

            AllWorlds.Add("default", Default);
            AllWorlds.Add("other", WorldOther());
            AllWorlds.Add("duel", WorldDuel());
            AllWorlds.Add("team", WorldTeam());
            AllWorlds.Add("ctf", WorldCTF());
            AllWorlds.Add("sharks", WorldSharks());
        }

        private static World WorldOther()
        {
            var hook = Hook.Default;
            hook.BotBase = 10;
            hook.BotRespawnDelay = 0;
            return new World
            {
                Hook = hook,
                Name = "Planet Daud",
                Description = "AAAAAHHH! Run!",
                AllowedColors = AllColors.Append("ship0").ToArray()
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
                Hook = hook,
                Name = "Dueling Room",
                Description = "1 vs. 1",
                AllowedColors = AllColors
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
                Hook = hook,
                Name = "Team",
                Description = "Cyan vs. Red",
                AllowedColors = TeamColors
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
                Hook = hook,
                Name = "Capture the Flag",
                Description = "Cyan vs. Red - Capture the Flag. First to 5 wins!",
                AllowedColors = TeamColors
            };
        }

        private static World WorldSharks()
        {
            var hook = Hook.Default;
            hook.BotBase = 0;
            hook.Obstacles = 0;
            hook.TeamMode = true;
            hook.PointsPerKillFleet = 1;
            hook.PointsPerKillShip = 0;
            hook.WorldSize /= 2;

            return new World
            {
                Hook = hook,
                Name = "Sharks and Minnows",
                Description = "Sharks vs. Minnows",
                AllowedColors = TeamColors,
                NewFleetGenerator = delegate (Player p, string Color)
                {
                    return new Fleet
                    {
                        Owner = p,
                        Caption = p.Name,
                        Shark = Color == "red",
                    };
                }
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

