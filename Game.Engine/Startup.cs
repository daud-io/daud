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
    using Game.Engine.Crypto.LetsEncrypt;
    using Game.Engine.Networking;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var config = LoadConfiguration(services);
            services.AddSingleton<GameConfiguration>(config);

            services.AddTransient<Networking.Connection>();

            services.UseJWTAuthentication();
            services.AddTransient<JWT, JWT>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ISecurityContext, TokenSecurityContext>();
            services.AddMvc().
                AddJsonOptions(options =>
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
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<DiscordBot>();


            services.AddSingleton(new RegistryClient(new Uri(config.RegistryUri)));
            if (config.RegistryEnabled)
                services.AddSingleton<RegistryHandling>();
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

            if (config.ForceHTTPS)
                app.UseHttpsRedirection();

            if (config.AllowCORS)
                app.UseCors();

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
            app.UseAcmeChallengeHandler();

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMilliseconds(10000)
            });
            app.UseGameWebsocketHandler();

            Worlds.Initialize(config);

            if (config.RegistryEnabled)
            {
                Console.WriteLine("Registry reporting is enabled");
                provider.GetService<RegistryHandling>();
            }
        }
    }
}
