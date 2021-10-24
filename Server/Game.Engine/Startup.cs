namespace Game.Engine
{
    using Discord.Commands;
    using Discord.WebSocket;
    using Game.API.Authentication;
    using Game.API.Client;
    using Game.API.Common.Security;
    using Game.Engine.Authentication;
    using Game.Engine.ChatBot;
    using Game.Engine.Core;
    using Game.Engine.Networking;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using Elasticsearch.Net;
    using Nest;
    using Nest.JsonNetSerializer;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.AspNetCore.Server.Kestrel.Core;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var config = LoadConfiguration(services);
            services.AddSingleton<GameConfiguration>(config);
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddTransient<Networking.Connection>();

            services.UseJWTAuthentication();
            services.AddTransient<JWT, JWT>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ISecurityContext, TokenSecurityContext>();
            services.AddMvc(options => options.EnableEndpointRouting = false).
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
                );
            });

            services.AddResponseCompression();

            services
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<DiscordBot>();


            services.AddSingleton(new RegistryClient(new Uri(config.RegistryUri)));
            if (config.RegistryEnabled)
                services.AddSingleton<RegistryHandling>();

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
            IWebHostEnvironment env,
            IServiceProvider provider,
            GameConfiguration config,
            RegistryClient registryClient
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
            app.UseResponseCompression();

            app.UseMvc();

            if (config.ForceHTTPS)
                app.UseHttpsRedirection();

            if (config.AllowCORS)
                app.UseCors("AllowAllOrigins");

            // Set up custom content types - associating file extension to MIME type
            var mimeProvider = new FileExtensionContentTypeProvider();
            mimeProvider.Mappings[".ts"] = "text/javascript";

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                HttpsCompression = HttpsCompressionMode.Compress,
                DefaultContentType = "text/plain",
                ContentTypeProvider = mimeProvider,
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                    context.Context.Response.Headers.Add("Expires", "-1");
                }
            });

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMilliseconds(10000)
            });
            app.UseGameWebsocketHandler();

            //RemoteEventLog.Initialize(config, registryClient);
            Worlds.Initialize(config);

            if (config.RegistryEnabled)
            {
                Console.WriteLine("Registry reporting is enabled");
                provider.GetService<RegistryHandling>();
            }
        }
    }
}
