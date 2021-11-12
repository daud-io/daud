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

        [HttpPut]
        public string Create(string worldKey, string hookJson)
        {
            var hook = Game.API.Common.Models.Hook.Default;

            PatchJSONIntoHook(hook, hookJson);
            var publicURL = GameConfiguration.PublicURL ?? Request.Host.ToString();
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
            //world.GetActor<RoomReset>().Reset = true;

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

        [AllowAnonymous, HttpGet, Route("list"), EnableCors("AllowAllOrigins")]
        public IEnumerable<string> GetWorlds()
        {

            return Worlds.AllWorlds.Select(w => w.Key);
        }
    }
}