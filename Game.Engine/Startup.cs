namespace Game.Engine
{
    using Game.Engine.Networking;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Net.Http.Headers;
    using System;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<Connection>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Use(async (httpContext, next) =>
            {
                httpContext.Response.Headers[HeaderNames.Pragma] = "no-cache";
                httpContext.Response.Headers[HeaderNames.CacheControl] = "no-cache";
                await next();
            });

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "text/plain"
            });

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMilliseconds(10000)
            });
            app.UseGameWebsocketHandler();
        }
    }
}
