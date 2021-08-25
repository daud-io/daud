namespace Game.Engine.Controllers
{
    using Game.API.Client;
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Game.Engine.Core;
    using Game.Engine.Core.SystemActors;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
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

        [HttpPut]
        public async Task<string> Create(string worldKey, string hookJson)
        {
            var hook = Game.API.Common.Models.Hook.Default;

            PatchJSONIntoHook(hook, hookJson);
            var publicURL = GameConfiguration.PublicURL ?? Request.Host.ToString();

            if (GameConfiguration.RegistryEnabled)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(15000);
                var suggestion = await RegistryClient.Registry.SuggestAsync(GameConfiguration.PublicURL, cts.Token);
                if (suggestion != "localhost")
                    publicURL = suggestion;
            }

            var world = new World(hook, GameConfiguration, worldKey);

            Worlds.AddWorld(world);

            return $"{publicURL}/{worldKey}";
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
        public async Task<Hook> Hook(string worldName = null)
        {
            string json = null;

            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                json = await reader.ReadToEndAsync();

            var world = Worlds.Find(worldName);

            JsonConvert.PopulateObject(json, world.Hook);

            // connection is using getHashCode for change detection
            world.Hook = world.Hook.Clone();

            return world.Hook;
        }

        [HttpPost, Route("reset")]
        public bool Reset(string worldName = null)
        {
            var world = Worlds.Find(worldName);
            world.GetActor<RoomReset>().Reset = true;

            return true;
        }


        [AllowAnonymous, HttpGet, Route("mesh/{worldKey}/{id}"), EnableCors("AllowAllOrigins")]
        public async Task<IActionResult> GetMesh(string worldKey, string id)
        {
            this.SuppressWrapper=true;
            var world = Worlds.Find(worldKey);
            var mesh = await world.MeshLoader.GetMeshStreamAsync(id);
            return File(mesh, "model/gltf-binary", "server.glb");
        }

        [AllowAnonymous, HttpGet, Route("all"), EnableCors("AllowAllOrigins")]
        public async Task<IEnumerable<object>> GetWorlds(string worldName = null, bool allWorlds = false)
        {
            var worlds = new List<object>();

            if (GameConfiguration.RegistryEnabled)
            {
                bool first = true;
                var serverWorlds = await RegistryClient.Registry.ListAsync();
                worlds.AddRange(
                    serverWorlds
                        .SelectMany(server => server.Worlds.Select(world => new { server, world }))
                        .OrderBy(s => s.world.Hook.Weight)
                        .Select(s =>
                        {
                            bool isDefault = first;
                            first = false;
                            return
                                new
                                {
                                    world = $"{s.server.URL}/{s.world.WorldKey}",
                                    server = s.server.URL,
                                    players = s.world.AdvertisedPlayers,
                                    name = s.world.Hook.Name,
                                    description = s.world.Hook.Description,
                                    allowedColors = s.world.Hook.AllowedColors,
                                    instructions = s.world.Hook.Instructions,
                                    isDefault = isDefault
                                };
                        })
                );
            }

            if (Request.HttpContext.Connection.LocalIpAddress.Equals(Request.HttpContext.Connection.RemoteIpAddress)
                || !GameConfiguration.RegistryEnabled)
            {
                bool first = true;
                worlds.AddRange(Worlds.AllWorlds
                        .OrderBy(w => w.Value.Hook.Weight)
                        .Select(s =>
                        {
                            bool isDefault = first;
                            first = false;

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
                                instructions = s.Value.Hook.Instructions,
                                isDefault = isDefault
                            };
                        }));
            }

            return worlds;
        }
    }
}