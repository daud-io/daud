namespace Game.Engine.ChatBot
{
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;

    public class DiscordBot
    {
        private GameConfiguration Config;
        private IServiceProvider Provider;

        public DiscordBot(GameConfiguration config, IServiceProvider provider)
        {
            this.Config = config;
            this.Provider = provider;
        }

        public async Task InitializeAsync()
        {
            if (Config.DiscordToken != null)
            {
                var client = Provider.GetRequiredService<DiscordSocketClient>();

                client.Log += LogAsync;
                Provider.GetRequiredService<CommandService>().Log += LogAsync;

                await client.LoginAsync(TokenType.Bot, Config.DiscordToken);
                await client.StartAsync();

                await Provider.GetRequiredService<CommandHandlingService>().InitializeAsync();
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
