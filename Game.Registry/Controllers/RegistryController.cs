namespace Game.Registry.Controllers
{
    using Game.API.Common.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

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
    }
}