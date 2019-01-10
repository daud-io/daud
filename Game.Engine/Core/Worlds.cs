namespace Game.Engine.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Worlds
    {
        public static readonly Dictionary<string, World> AllWorlds = new Dictionary<string, World>();

        private static readonly World Default;

        private static readonly string[] AllColors = new[] {
            "ship_pink",
            "ship_red",
            "ship_orange",
            "ship_yellow",
            "ship_green",
            "ship_cyan"
        };
        private static readonly string[] TeamColors = new[] {
            "ship_red",
            "ship_cyan"
        };

        static Worlds()
        {
            Default = WorldDefault();
            AllWorlds.Add("default", Default);

            AllWorlds.Add("other", WorldOther());
            AllWorlds.Add("duel", WorldDuel());
            AllWorlds.Add("team", WorldTeam());
            AllWorlds.Add("ctf", WorldCTF());
            AllWorlds.Add("sharks", WorldSharks());
            AllWorlds.Add("snake", WorldSnake());
        }

        private static World WorldDefault()
        {
            var hook = Hook.Default;

            return new World
            {
                Hook = hook,
                Name = "FFA",
                Description = "FFA Arena",
                AllowedColors = AllColors,
                Instructions = "Mouse to aim, click to shoot. Press 's' to boost."
            };
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

        private static World WorldSnake()
        {
            var hook = Hook.Default;
            hook.BotBase = 1;
            hook.FlockWeight = 0;
            hook.SnakeWeight = 0.01f;
            hook.FlockWeight = 0.02f;
            hook.FlockCohesion = 0.0003f;
            hook.FlockAlignment = 0;
            hook.FollowFirstShip = true;
            hook.FiringSequenceDelay = 250;

            return new World
            {
                Hook = hook,
                Name = "Snake World",
                Description = "Hisssssss...",
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
                Instructions = @"<p>features two teams,cyan and red, 
                    who each try to steal the other team's
                    flag and bring it back to their own 
                    base to 'capture'.</p>
                    <p>each team will have their own base and flag to defend.In order to score, your team's flag must still be 
                    at your base, which means you'll have to have some good defense to keep
                    the other team from running off with your flag.</p>
                    <p>If someone makes off with your flag, frag them and they'll drop your flag -- 
                    touch the flag and it will be returned
                    to your base.</p>",
                Image = "ctf",
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
            hook.PointsMultiplierDeath = 1.0f;
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

