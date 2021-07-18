namespace Game.Engine
{
    using Game.Engine.ChatBot;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
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

            if (cts.IsCancellationRequested)
                return;

            var bot = host.Services.GetService(typeof(DiscordBot)) as DiscordBot;
            await bot.InitializeAsync();

            await host.RunAsync(cts.Token);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            ThreadPool.SetMinThreads(50, 50);

            var builder = WebHost.CreateDefaultBuilder(args);

            var configContext = Configuration<GameConfiguration>.Load("config");

            builder.UseConfiguration(configContext.ConfigurationRoot);

            var port = System.Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port))
                builder.UseUrls($"http://*:{port}");

            builder
                .UseWebRoot("wwwroot/dist")
                .UseStartup<Startup>();

            return builder;
        }
    }
}
