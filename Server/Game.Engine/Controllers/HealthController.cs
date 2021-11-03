namespace Game.Engine.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/v1/[controller]")]
    public class HealthController : Controller
    {
        [
            AllowAnonymous,
            HttpGet
        ]
        public IActionResult GetHealth()
        {
            return Ok();
        }
    }
}
