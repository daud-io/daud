namespace Game.Engine.Core
{
    using Game.API.Common.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class Worlds
    {
        public static readonly Dictionary<string, World> AllWorlds = new Dictionary<string, World>();
        private static GameConfiguration GameConfiguration;
        public async static Task Initialize(GameConfiguration gameConfiguration)
        {
            GameConfiguration = gameConfiguration;

            if (gameConfiguration.WorldInitialization != null)
            {
                using var http = new HttpClient();
                
                foreach (var worldConfiguration in gameConfiguration.WorldInitialization)
                {
                    var hookJson = await http.GetStringAsync(worldConfiguration.HookURL);
                    var hook = Hook.Default;
                    JsonConvert.PopulateObject(hookJson, hook);

                    var world = new World(hook, GameConfiguration, worldConfiguration.WorldKey);
                    Worlds.AddWorld(world);
                }
            }
            
            if (!gameConfiguration.NoWorlds)
            {
                AddWorld(PartyCity(), "partycity");
                AddWorld(FFA(), "ffa");
            }
        }
        
        private static World FFA()
        {
            var hook = Hook.Default;
            hook.Name = "FFA";
            hook.Description = "Free-For-All: Kill the bad guys... they are all bad.";
            hook.Mesh.Enabled = true;
            hook.Mesh.MeshURL = "https://static.daud.io/worlds/ffa/ffa.glb";
            hook.WorldSize = 5000;

            return new World(hook, GameConfiguration, "ffa");
        }
        private static World Untitled()
        {
            var hook = Hook.Default;
            hook.Name = "untitled";
            hook.Description = "untitled";
            hook.Mesh.Enabled = true;
            hook.Mesh.MeshURL = "wwwroot/public/models/untitled.glb";
            hook.Mesh.Scale = new System.Numerics.Vector3(100, 100, 100);
            hook.WorldSize = 5000;

            return new World(hook, GameConfiguration, "untitled");
        }

        private static World Team()
        {
            var hook = Hook.Default;
            hook.Name = "Team";
            hook.TeamMode = true;
            hook.Description = "Teams Arena";
            hook.Mesh.Enabled = true;
            hook.Mesh.MeshURL = "wwwroot/public/models/ffa.glb";
            hook.WorldSize = 5000;
            hook.AllowedColors = Hook.TeamColors;

            return new World(hook, GameConfiguration, "team");
        }

        private static World PartyCity()
        {
            var hook = Hook.Default;
            hook.Name = "Party City";
            hook.Description = "Party City, what can we say?";
            hook.Mesh.Enabled = true;
            hook.Fishes *= 4;
            hook.PickupSeekers *= 4;
            hook.PickupShields *= 4;
            hook.Mesh.MeshURL = "wwwroot/public/models/partycity.glb";
            hook.WorldSize = 10000;

            return new World(hook, GameConfiguration, "partycity");
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

        public static void AddWorld(World world, string worldKey = null)
        {
            AllWorlds.Add(worldKey ?? world.WorldKey, world);
        }
/*
        private static World WorldDefault()
        {
            var hook = Hook.Default;
            hook.Name = "FFA";
            hook.Description = "FFA Arena";
            hook.Instructions = "Mouse to aim, click to shoot. Press 's' to boost.";
            hook.Weight = 10;
            hook.WorldSize = 4000;

            return new World(hook, GameConfiguration);
        }

        private static World RoboTrainer()
        {
            var hook = Hook.Default;
            hook.Name = "Robo Trainer";
            hook.Description = "Battle against bots of different difficulty levels";
            hook.AllowedColors = Hook.AllColors;
            hook.Weight = 100;

            return new World(hook, GameConfiguration);
        }

        private static World WorldSumo()
        {
            var hook = Hook.Default;
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

            return new World(hook, GameConfiguration);
        }

        private static World WorldDuel()
        {
            var hook = Hook.Default;
            hook.WorldSize = 4200;
            hook.Obstacles = 3;
            hook.Fishes = 7;
            hook.PickupSeekers = 3;
            hook.PickupShields = 0;
            hook.PointsPerKillFleet = 1;
            hook.PointsPerKillFleetMax = 1;
            hook.PointsPerKillShip = 0;
            hook.PointsPerUniverseDeath = -1;
            hook.PointsMultiplierDeath = 1.0f;
            hook.PointsPerKillFleetStep = 1;
            hook.PointsPerKillFleetPerStep = 1;
            hook.ComboPointsStep = 0;
            hook.ComboDelay = 0;

            hook.Weight = 20;

            hook.Name = "Dueling Room";
            hook.Description = "1 vs. 1";

            return new World(hook, GameConfiguration);
        }

        private static World WorldTeam()
        {
            var hook = Hook.Default;
            hook.Obstacles = 3;
            hook.TeamMode = true;
            hook.Weight = 20;

            hook.Name = "Team";
            hook.Description = "Cyan vs. Red";
            hook.AllowedColors = Hook.TeamColors;

            return new World(hook, GameConfiguration);
        }

        private static World WorldCTF()
        {
            var hook = Hook.Default;
            hook.Obstacles = 7;
            hook.CTFMode = true;
            hook.PointsPerKillFleet = 1;
            hook.PointsPerKillShip = 0;
            hook.PointsPerUniverseDeath = -1;
            hook.PointsMultiplierDeath = 1.0f;
            hook.Weight = 20;
            hook.SpawnLocationMode = "CTF";

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

            return new World(hook, GameConfiguration);
        }

        private static World WorldRoyale()
        {
            var hook = Hook.Default;
            hook.RoyaleMode = true;
            //hook.WorldSizeBasic = 0;
            //hook.WorldSizeDeltaPerPlayer = 1600;
            //hook.WorldResizeSpeed = 50;

            hook.Weight = 11;

            hook.Name = "Battle Royale Mode";
            hook.Description = "Some stuff happens here";
            hook.Instructions = @"<p>Try not to die</p>";

            return new World(hook, GameConfiguration);
        }*/

        public static World Find(string world)
        {
            return AllWorlds[world];
        }
    }
}

