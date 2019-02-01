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
        static readonly IEnumerable<string> DefaultDnsNames = new[] {
            "27.109.229.35.bc.googleusercontent.com",
            "test-alt1.example.com",
            "test-alt21.example.com"
        };

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

            var port = System.Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port)) 
                builder = builder.UseUrls($"http://*:{port}");
            else
                builder = builder.UseConfiguration(new ConfigurationBuilder()
                    .AddJsonFile("hosting.json", optional: true)
                    .Build()
                );

            builder
                .UseWebRoot("wwwroot/dist")
                .UseStartup<Startup>();


            // Full Form with access to All Options:
            builder.AddAcmeServices(new AcmeOptions
            {
                AcmeRootDir = "_IGNORE/_acmesharp",
                DnsNames = Program.DnsNames ?? DefaultDnsNames,
                AccountContactEmails = new[] { "game@violetdata.com" },
                AcceptTermsOfService = true,
                CertificateKeyAlgor = "rsa",
            });

            return builder;
        }
    }
}
