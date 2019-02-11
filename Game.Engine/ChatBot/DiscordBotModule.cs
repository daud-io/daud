namespace Game.Engine.ChatBot
{
    using Docker.DotNet;
    using Discord;
    using Discord.Commands;
    using Discord.Rest;
    using Game.Engine.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Docker.DotNet.Models;

    // Modules must be public and inherit from an IModuleBase
    public class DiscordBotModule : ModuleBase<SocketCommandContext>
    {

        private static Dictionary<string, RestSelfUser> TokenUserMap = new Dictionary<string, RestSelfUser>();
        private static Dictionary<ulong, RestSelfUser> IDUserMap = new Dictionary<ulong, RestSelfUser>();
        private readonly GameConfiguration GameConfiguration;

        public DiscordBotModule(GameConfiguration gameConfiguration)
        {
            GameConfiguration = gameConfiguration;
        }

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("stop it!");

        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;

            await ReplyAsync(user.ToString());
        }

        [Command("reset"), RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task ResetAsync()
        {
            
            Program.Abort();
            await ReplyAsync("woah... room spinning. so... cold...");
        }

        [Command("deploy"), RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task DeployAsync(string url, string tag)
        {
            if (url == GameConfiguration.PublicURL)
            {
                DockerClient client = new DockerClientConfiguration(
                    new Uri("unix:///var/run/docker.sock"))
                     .CreateClient();

                var container = await client.Containers.InspectContainerAsync(Environment.MachineName);

                var config = container.Config;
                var oldImage = config.Image;
                config.Image = $"iodaud/daud:{tag}";

                var response = await client.Containers.CreateContainerAsync(new CreateContainerParameters(config));
                if (response.Warnings.Count == 0)
                    await client.Containers.RemoveContainerAsync(Environment.MachineName, new ContainerRemoveParameters
                    {
                        Force = true
                    });

                await ReplyAsync($"{GameConfiguration.PublicURL} {oldImage}->{config.Image}");

                Program.Abort();
            }
        }


        [Command("worlds")]
        public async Task WorldsAsync()
        {
            var response = "*worlds report*\n";
            using (var drc = new DiscordRestClient())
            {

                foreach (var world in Worlds.AllWorlds)
                {
                    var players = Player.GetWorldPlayers(world.Value);

                    foreach (var player in players)
                    {
                        if (!string.IsNullOrWhiteSpace(player.Token))
                        {
                            try
                            {
                                await drc.LoginAsync(TokenType.Bearer, player.Token);
                                response += $"{world.Key}({world.Value.AdvertisedPlayerCount}): {player.Name} is @{drc.CurrentUser}\n";
                            }
                            catch (Exception e)
                            {
                                response += $"{world.Key}({world.Value.AdvertisedPlayerCount}): {player.Name} FAIL: ${e.Message}\n";
                            }
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

        public static Task WorldAnnounce(World world, string message)
        {
            foreach (var player in Player.GetWorldPlayers(world))
                player.SendMessage(message);

            return Task.FromResult(0);
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
                    try
                    {
                        using (var drc = new DiscordRestClient())
                        {
                            await drc.LoginAsync(TokenType.Bearer, token);
                            TokenUserMap.Add(token, drc.CurrentUser);
                            IDUserMap.Add(drc.CurrentUser.Id, drc.CurrentUser);
                            return drc.CurrentUser;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Exception while Getting Discord User: {e}");
                    }
                }
            }


            return null;
        }
    }
}
