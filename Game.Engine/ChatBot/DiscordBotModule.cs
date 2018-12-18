namespace Game.Engine.ChatBot
{
    using Discord;
    using Discord.Commands;
    using Discord.Rest;
    using Game.Engine.Core;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    // Modules must be public and inherit from an IModuleBase
    public class DiscordBotModule : ModuleBase<SocketCommandContext>
    {
        private static Dictionary<string, RestSelfUser> TokenUserMap = new Dictionary<string, RestSelfUser>();
        private static Dictionary<ulong, RestSelfUser> IDUserMap = new Dictionary<ulong, RestSelfUser>();

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
                            response += $" - {player.Name} @{drc.CurrentUser}\n";
                        }
                        else
                        {
                            response += $" - {player.Name}\n";
                        }
                    }

                }
            }
            response += "\n";


            await ReplyAsync(response);
        }

        private static async Task ScanAllSignedInPlayers()
        {
            foreach (var world in Worlds.AllWorlds)
            {
                foreach (var player in Player.GetWorldPlayers(world.Value))
                {
                    await GetUser(player.Token);
                }
            }
        }


        public static async Task UserMentions(IEnumerable<ulong> ids, string message)
        {
            foreach (var world in Worlds.AllWorlds)
            {
                var players = Player.GetWorldPlayers(world.Value);
                foreach (var player in players)
                {
                    var user = await GetUser(player.Token);
                    if (user != null && ids.Any(i => i == user.Id))
                    {
                        player.SendMessage(message);
                    }
                }
            }
        }

        private static RestSelfUser GetUser(ulong id)
        {
            return IDUserMap.ContainsKey(id)
                ? IDUserMap[id]
                : null;
        }

        private static async Task<RestSelfUser> GetUser(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {

                if (TokenUserMap.ContainsKey(token))
                {
                    return TokenUserMap[token];
                }
                else
                {
                    using (var drc = new DiscordRestClient())
                    {
                        await drc.LoginAsync(TokenType.Bearer, token);
                        TokenUserMap.Add(token, drc.CurrentUser);
                        IDUserMap.Add(drc.CurrentUser.Id, drc.CurrentUser);
                        return drc.CurrentUser;
                    }
                }
            }


            return null;
        }
    }
}
