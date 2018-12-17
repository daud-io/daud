namespace Game.Engine.ChatBot
{
    using Discord;
    using Discord.Commands;
    using Discord.Rest;
    using Game.Engine.Core;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    // Modules must be public and inherit from an IModuleBase
    public class DiscordBotModule : ModuleBase<SocketCommandContext>
    {
        private static Dictionary<string, ulong> TokenUserMap = new Dictionary<string, ulong>();

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");

        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;

            await ReplyAsync(user.ToString());
        }

        [Command("worlds")]
        public async Task WorldsAsync()
        {
            var response = "\n";
            using (var drc = new DiscordRestClient())
            {

                foreach (var world in Worlds.AllWorlds)
                {
                    response += $"{world.Key} {world.Value.AdvertisedPlayerCount}\n";

                    var players = Player.GetWorldPlayers(world.Value);

                    foreach (var player in players)
                    {
                        if (!string.IsNullOrWhiteSpace(player.Token))
                        {
                            await drc.LoginAsync(TokenType.Bearer, player.Token);
                            response += $" - {player.Name} <@{drc.CurrentUser.Id}>\n";
                        }
                        else
                            response += $" - {player.Name}\n";
                    }

                }
            }
            response += "\n";


            await ReplyAsync(response);
        }

        
        public static async Task UserMentions(IEnumerable<ulong> ids, string message)
        {
            foreach (var world in Worlds.AllWorlds)
            {

                var players = Player.GetWorldPlayers(world.Value);
                foreach (var player in players)
                {
                    var id = await GetUserID(player.Token);
                    if (id != 0)
                        player.SendMessage(message);
                }
            }
        }

        private static async Task<ulong> GetUserID(string token)
        {
            if (TokenUserMap.ContainsKey(token))
                return TokenUserMap[token];
            else
                using (var drc = new DiscordRestClient())
                {
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        await drc.LoginAsync(TokenType.Bearer, token);
                        TokenUserMap.Add(token, drc.CurrentUser.Id);
                        return drc.CurrentUser.Id;
                    }
                }

            return 0;
        }
    }
}
