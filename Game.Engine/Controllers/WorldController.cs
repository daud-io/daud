namespace Game.Engine.Controllers
{
    using Game.API.Client;
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Game.Engine.Core;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class WorldController : APIControllerBase
    {
        private readonly RegistryClient RegistryClient;
        private readonly GameConfiguration GameConfiguration;

        public WorldController(ISecurityContext securityContext,
            RegistryClient registryClient, GameConfiguration gameConfiguration) : base(securityContext)
        {
            this.RegistryClient = registryClient;
            this.GameConfiguration = gameConfiguration;

        }

        [HttpPost, Route("map")]
        public bool SetMap([FromBody] IEnumerable<MapTileModel> tiles, string worldKey)
        {
            var world = Worlds.Find(worldKey);
            if (world != null)
            {
                world.MapActor.SetTiles(tiles);
                return true;
            }
            else
                return false;
        }

        [HttpPut]
        public string Create(string worldKey, string hookJson)
        {

            var hook = Hook.Default;

            PatchJSONIntoHook(hook, hookJson);

            var world = new World
            {
                Hook = hook,
                WorldKey = worldKey
            };

            Worlds.AddWorld(world);

            return worldKey;
        }

        [HttpDelete]
        public string Delete(string worldKey)
        {
            Worlds.Destroy(worldKey);
            return worldKey;
        }

        private void PatchJSONIntoHook(Hook hook, string json)
        {
            JsonConvert.PopulateObject(json, hook);
        }

        [HttpPost, Route("hook")]
        public async Task<string> PostHook(string worldName = null)
        {
            string json = null;

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                json = await reader.ReadToEndAsync();

            var world = Worlds.Find(worldName);

            PatchJSONIntoHook(world.Hook, json);

            // connection is using getHashCode for change detection
            world.Hook = world.Hook.Clone();

            return JsonConvert.SerializeObject(world.Hook, Formatting.Indented);
        }

        [AllowAnonymous, HttpGet, Route("all"), EnableCors("AllowAllOrigins")]
        public async Task<IEnumerable<object>> GetWorlds(string worldName = null, bool allWorlds = false)
        {
            var worlds = new List<object>();

            if (GameConfiguration.RegistryEnabled)
            {
                var serverWorlds = await RegistryClient.Registry.ListAsync();
                worlds.AddRange(
                    serverWorlds
                        .Where(s => new[] { "de.daud.io", "us.daud.io" }.Contains(s.URL))
                        .SelectMany(server => server.Worlds.Select(world => new { server, world }))
                        .Where(s => allWorlds || !s.world.Hook.Hidden)
                        .Where(s => s.server.URL == "us.daud.io" || (s.server.URL == "de.daud.io" && s.world.WorldKey == "default"))
                        .OrderBy(s => s.world.Hook.Weight)
                        .Select(s =>
                        {
                            var name = s.world.Hook.Name;
                            var description = s.world.Hook.Description;

                            if (name == "FFA" && s.server.URL == "de.daud.io")
                            {
                                name = "FFA - Europe";
                                description = "Like regular FFA but with different ping times and metric-sized cup holders";
                            }
                            return
                                new
                                {
                                    world = $"{s.server.URL}/{s.world.WorldKey}",
                                    server = s.server.URL,
                                    players = s.world.AdvertisedPlayers,
                                    name,
                                    description,
                                    allowedColors = s.world.Hook.AllowedColors,
                                    instructions = s.world.Hook.Instructions
                                };
                        })
                );
            }

            if (Request.HttpContext.Connection.LocalIpAddress.Equals(Request.HttpContext.Connection.RemoteIpAddress))
            {
                worlds.AddRange(Worlds.AllWorlds
                        .OrderBy(w => w.Value.Hook.Weight)
                        .Select(s =>
                        {
                            var name = s.Value.Hook.Name;
                            var description = s.Value.Hook.Description;

                            return new
                            {
                                world = $"{Request.Host}/{s.Value.WorldKey}",
                                server = Request.Host,
                                players = s.Value.AdvertisedPlayerCount,
                                name = "Local: " + name,
                                description,
                                allowedColors = s.Value.Hook.AllowedColors,
                                instructions = s.Value.Hook.Instructions
                            };
                        }));
            }

            return worlds;
        }
    }
}