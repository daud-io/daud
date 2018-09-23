namespace Game.Engine
{
    using Game.API.Authentication;
    using Game.API.Common.Security;
    using Game.Engine.Networking;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using System;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var config = LoadConfiguration(services);

            services.AddTransient<Connection>();

            services.UseJWTAuthentication();
            services.AddTransient<JWT, JWT>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ISecurityContext, TokenSecurityContext>();
            services.AddMvc();
        }

        private GameConfiguration LoadConfiguration(IServiceCollection services)
        {
            var config = Configuration<GameConfiguration>.Load("config");

            services.AddSingleton(config.Object);

            return config.Object;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Use(async (httpContext, next) =>
            {
                httpContext.Response.Headers[HeaderNames.Pragma] = "no-cache";
                httpContext.Response.Headers[HeaderNames.CacheControl] = "no-cache";
                await next();
            });

            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };
            };

            app.UseMvc();
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
