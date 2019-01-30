namespace Game.Registry.Controllers
{
    using Game.API.Common.Security;
    using Microsoft.AspNetCore.Mvc;

    public class ServerController : APIControllerBase
    {
        public ServerController(ISecurityContext securityContext)
             : base(securityContext)
        {
        }

        [HttpPost, Route("reset")]
        public bool Reset()
        {
            Program.Abort();

            return true;
        }
    }
}
