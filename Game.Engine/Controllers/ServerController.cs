namespace Game.Engine.Controllers
{
    using Game.API.Common.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    public class ServerController : APIControllerBase
    {
        public ServerController(ISecurityContext securityContext)
             : base(securityContext)
        {
        }

        [HttpGet, AllowAnonymous]
        public bool Get()
        {
            return true;
        }
    }
}
