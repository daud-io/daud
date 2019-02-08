namespace Game.Engine
{
    using Game.Engine.ChatBot;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();
        static IEnumerable<string> DnsNames { get; set; }

        public static void Abort()
        {
            cts.Cancel();
        }

        public async static Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            var bot = host.Services.GetService(typeof(DiscordBot)) as DiscordBot;
            await bot.InitializeAsync();

            await host.RunAsync(cts.Token);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            var config = new GameConfiguration();
            Configuration<GameConfiguration>.Load("config", instance: config);

            var port = System.Environment.GetEnvironmentVariable("PORT");

            if (!string.IsNullOrEmpty(port))
            {
                builder.UseUrls($"http://*:{port}");
            }
            else
                builder.UseConfiguration(new ConfigurationBuilder()
                    .AddJsonFile("hosting.json", optional: true)
                    .Build()
                );

            builder
                .UseWebRoot("wwwroot/dist")
                .UseStartup<Startup>();


            if (config.LetsEncryptEnabled && config.RegistryEnabled)
                // Full Form with access to All Options:
                builder.AddAcmeServices(new AcmeOptions
                {
                    AcmeRootDir = "_IGNORE/_acmesharp",
                    AccountContactEmails = new[] { "info@daud.io" },
                    AcceptTermsOfService = true,
                    CertificateKeyAlgor = "rsa",
                });

            return builder;
        }
    }
}
