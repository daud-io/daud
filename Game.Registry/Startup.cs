namespace Game.Registry
{
    using Elasticsearch.Net;
    using Game.API.Common.Security;
    using Game.Registry.API.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Nest;
    using Nest.JsonNetSerializer;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var config = LoadConfiguration(services);
            services.AddSingleton<GameConfiguration>(config);

            services.UseJWTAuthentication();
            services.AddTransient<JWT, JWT>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ISecurityContext, TokenSecurityContext>();
            services.AddMvc().
                AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                );
            });

            services
                .AddSingleton<HttpClient>();

            if (config.ElasticSearchURI != null)
            {

                // choose the appropriate IConnectionPool for your use case
                var pool = new SingleNodeConnectionPool(new Uri(config.ElasticSearchURI));
                var connectionSettings =
                    new ConnectionSettings(pool, JsonNetSerializer.Default)
                    .DefaultIndex("daud");
                services.AddSingleton(new ElasticClient(connectionSettings));
            }
            else
                services.AddSingleton(new ElasticClient());
        }

        private GameConfiguration LoadConfiguration(IServiceCollection services)
        {
            var config = Configuration<GameConfiguration>.Load("config");

            services.AddSingleton(config.Object);

            return config.Object;
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IServiceProvider provider,
            GameConfiguration config
        )
        {
            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };
            };

            app.UseAuthentication();

            app.UseMvc();

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "text/plain",
                OnPrepareResponse = context =>
                {
                    if (context.File.Name == "index.html")
                    {
                        context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                        context.Context.Response.Headers.Add("Expires", "-1");
                    }
                }
            });

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMilliseconds(10000)
            });

        }
    }
}
