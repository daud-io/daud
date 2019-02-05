namespace Game.Engine.Core
{
    using Game.API.Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Worlds
    {
        public static readonly Dictionary<string, World> AllWorlds = new Dictionary<string, World>();

        private static World Default;

        public static void Initialize()
        {
            Default = WorldDefault();
            AddWorld("default", Default);

            AddWorld("other", WorldOther());
            AddWorld("duel", WorldDuel());
            AddWorld("team", WorldTeam());
            AddWorld("ctf", WorldCTF());
            AddWorld("sharks", WorldSharks());
            AddWorld("sumo", WorldSumo());
            AddWorld("boss", WorldBoss());
            //AddWorld("wormhole", WorldWormhole());
            //AddWorld("beach", WorldBeach());
        }

        public static void Destroy(string worldKey)
        {
            var world = Find(worldKey);

            if (world != null && world.WorldKey == worldKey)
                Destroy(world);
        }

        public static void Destroy(World world)
        {
            try
            {
                if (AllWorlds.ContainsKey(world.WorldKey))
                    AllWorlds.Remove(world.WorldKey);
            }
            catch (Exception) { }
            try
            {
                ((IDisposable)world).Dispose();
            }
            catch (Exception) { }
        }

        public static void AddWorld(World world)
        {
            AllWorlds.Add(world.WorldKey, world);
        }

        public static void AddWorld(string worldKey, World world)
        {
            world.WorldKey = worldKey;
            AllWorlds.Add(world.WorldKey, world);
        }


        private static World WorldDefault()
        {
            var hook = Hook.Default;
            hook.Name = "FFA";
            hook.Description = "FFA Arena";
            hook.Instructions = "Mouse to aim, click to shoot. Press 's' to boost.";
            hook.Weight = 10;

            return new World(hook);
        }

        private static World WorldOther()
        {
            var hook = Hook.Default;
            hook.BotBase = 10;
            hook.BotRespawnDelay = 0;
            hook.PickupShields = 10;

            hook.Name = "Planet Daud";
            hook.Description = "AAAAAHHH! Run!";
            hook.AllowedColors = Hook.AllColors.Append("ship0").ToArray();
            hook.Weight = 100;

            return new World(hook);
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

            hook.Name = "Snake World";
            hook.Description = "Hisssssss...";
            hook.AllowedColors = Hook.AllColors.Append("ship0").ToArray();

            return new World(hook);
        }

        private static World WorldSumo()
        {
            var hook = Hook.Default;
            hook.BotBase = 0;
            hook.WorldSize = 1500;
            hook.Obstacles = 0;
            hook.Fishes = 20;
            hook.PickupSeekers = 0;
            hook.SpawnInvulnerabilityTime = 0;
            hook.PickupShields = 0;
            hook.SpawnShipCount = 10;
            hook.PointsPerKillFleet = 1;
            hook.PointsPerKillShip = 0;
            hook.PointsPerUniverseDeath = 0;
            hook.PointsMultiplierDeath = 1.0f;
            hook.SumoMode = true;
            hook.SumoRingSize = 1000;
            hook.Weight = 100;


            hook.Name = "Sumo World";
            hook.Description = "Bigger Better...";

            return new World(hook);
        }

        private static World WorldDuel()
        {
            var hook = Hook.Default;
            hook.BotBase = 0;
            hook.WorldSize = 4200;
            hook.Obstacles = 3;
            hook.Fishes = 7;
            hook.PickupSeekers = 3;
            hook.PickupShields = 0;
            hook.PointsPerKillFleet = 1;
            hook.PointsPerKillShip = 0;
            hook.PointsPerUniverseDeath = -1;
            hook.PointsMultiplierDeath = 1.0f;
            hook.Weight = 20;

            hook.Name = "Dueling Room";
            hook.Description = "1 vs. 1";

            return new World(hook);
        }

        private static World WorldTeam()
        {
            var hook = Hook.Default;
            hook.BotBase = 0;
            hook.Obstacles = 3;
            hook.TeamMode = true;
            hook.Weight = 20;

            hook.Name = "Team";
            hook.Description = "Cyan vs. Red";
            hook.AllowedColors = Hook.TeamColors;

            return new World(hook);
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
            hook.Weight = 20;

            hook.Name = "Capture the Flag";
            hook.Description = "Cyan vs. Red - Capture the Flag. First to 5 wins!";
            hook.Instructions = @"<p>features two teams,cyan and red, 
                    who each try to steal the other team's
                    flag and bring it back to their own 
                    base to 'capture'.</p>
                    <p>each team will have their own base and flag to defend.In order to score, your team's flag must still be 
                    at your base, which means you'll have to have some good defense to keep
                    the other team from running off with your flag.</p>
                    <p>If someone makes off with your flag, frag them and they'll drop your flag -- 
                    touch the flag and it will be returned
                    to your base.</p>";

            hook.AllowedColors = Hook.TeamColors;

            return new World(hook);
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
            hook.Weight = 100;

            hook.Name = "Sharks and Minnows";
            hook.Description = "Sharks and Minnows";
            hook.Instructions = "how to score:<br><br>"
                    + " - Sharks (red) hunt<br>"
                    + " - Minnows (blue) run towards borders (left & right)";

            hook.AllowedColors = Hook.TeamColors;

            return new World
            {
                Hook = hook,
                NewFleetGenerator = delegate (Player p, string Color)
                {
                    return new Fleet
                    {
                        Owner = p,
                        Caption = p.Name,
                        Color = Color,
                        Shark = Color == "red",
                    };
                }
            };
        }

        private static World WorldWormhole()
        {
            var hook = Hook.Default;
            hook.WorldSize = 1000;
            hook.BotBase = 0;
            hook.Obstacles = 0;
            hook.Wormholes = 1;
            hook.WormholesDestination = "duel";
            hook.Name = "Wormhole test";
            hook.Description = "Wormhole test";
            hook.AllowedColors = Hook.TeamColors;
            hook.Weight = 1000;

            return new World(hook);
        }

        private static World WorldBoss()
        {
            var hook = Hook.Default;
            hook.BotBase = 3;
            hook.BossMode = true;
            hook.BossModeSprites = new API.Common.Sprites[] { API.Common.Sprites.ship0 };
            hook.ShotCooldownTimeBotB = 200;
            hook.SpawnShipCount = 3;
            hook.Name = "Boss Mode";
            hook.Description = "So many Circles! Much wow!";
            hook.AllowedColors = Hook.AllColors.Append("ship0").ToArray();
            hook.Weight = 100;

            return new World(hook);
        }

        private static World WorldBeach()
        {
            var hook = Hook.Default;
            hook.BotBase = 0;
            hook.MapEnabled = true;
            hook.SpawnLocationMode = "Static";
            hook.Name = "Beach World";
            hook.Description = "Come on in, the water's fine";
            hook.Weight = 1000;

            return new World(hook);
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

