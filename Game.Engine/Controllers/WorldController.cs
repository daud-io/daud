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

        public WorldController(ISecurityContext securityContext,
            RegistryClient registryClient) : base(securityContext)
        {
            this.RegistryClient = registryClient;
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
            var serverWorlds = await RegistryClient.Registry.ListAsync();

            return serverWorlds
                .Where(s => new[] { "daud.io", "de.daud.io", "ca.daud.io" }.Contains(s.URL))
                .SelectMany(server => server.Worlds.Select(world => new { server, world }))
                .Where(s => allWorlds || !s.world.Hook.Hidden)
                .Where(s => s.server.URL == "daud.io" || (s.server.URL == "de.daud.io" && s.world.WorldKey == "ffa"))
                .OrderBy(s => s.world.Hook.Weight)
                .Select(s => new
                {
                    world = $"{s.server.URL}/{s.world.WorldKey}",
                    players = s.world.AdvertisedPlayers,
                    name = s.world.Hook.Name,
                    description = s.world.Hook.Description,
                    allowedColors = s.world.Hook.AllowedColors,
                    instructions = s.world.Hook.Instructions
                });
        }

    }
}