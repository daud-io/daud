namespace Game.Engine
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();

        public static void Abort()
        {
            cts.Cancel();
        }

        public async static Task Main(string[] args)
        {
            await CreateWebHostBuilder(args).Build().RunAsync(cts.Token);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            var port = System.Environment.GetEnvironmentVariable("$PORT");
            if (!string.IsNullOrEmpty(port))
                builder = builder.UseUrls($"http://*:{port}");
            else
                builder = builder.UseConfiguration(new ConfigurationBuilder()
                    .AddJsonFile("hosting.json", optional: true)
                    .Build()
                );

            return builder
                .UseStartup<Startup>();
        }
    }
}
