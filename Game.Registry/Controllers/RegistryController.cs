namespace Game.Registry.Controllers
{
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;

    public class RegistryController : APIControllerBase
    {
        private readonly GameConfiguration Config;

        public RegistryController(
            ISecurityContext securityContext,
            GameConfiguration config
        ) : base(securityContext)
        {
            this.Config = config;
        }

        [
            AllowAnonymous,
            HttpGet
        ]
        public bool GetList()
        {
            return true;
        }

        [
            AllowAnonymous,
            HttpPost,
            Route("report")
        ]
        public bool PostReportAsync([FromBody]RegistryReport registryReport)
        {

            Console.WriteLine(JsonConvert.SerializeObject(registryReport));
            return true;
        }
    }
}