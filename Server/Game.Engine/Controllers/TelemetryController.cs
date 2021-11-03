namespace Game.Engine.Controllers
{
    using Game.API.Common.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.IO;

    public class TelemetryController : APIControllerBase, IDisposable
    {
        public GameConfiguration Configuration { get; }
        public static Stream TelemetryFileStream;

        public TelemetryController(
            ISecurityContext securityContext,
            GameConfiguration configuration
        ) : base(securityContext)
        {
            Configuration = configuration;

            if (configuration.TelemetryFile != null && TelemetryFileStream == null)
                TelemetryFileStream = System.IO.File.OpenWrite(configuration.TelemetryFile);

        }

        [
            AllowAnonymous,
            HttpPost
        ]
        public IActionResult PostTelemetry()
        {
            this.SuppressWrapper = true;
            if (Request.ContentLength < 10000 && TelemetryFileStream != null)
                lock(TelemetryFileStream)
                {
                    Request.Body.CopyTo(TelemetryFileStream);
                    TelemetryFileStream.WriteByte(10);
                    TelemetryFileStream.Flush();
                }

            return Ok(true);
        }
    }
}
