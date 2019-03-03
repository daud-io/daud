namespace Game.Engine.Core.SystemActors
{
    using Discord.Rest;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Authenticator : SystemActorBase
    {
        public Authenticator()
        {
            this.CycleMS = 0;
        }

        protected override void CycleThink()
        {
            if (World.GameConfiguration.DiscordGuildID != null && World.GameConfiguration.DiscordToken != null)
            {
                foreach (var player in Player.GetWorldPlayers(this.World)
                    .Where(p => !string.IsNullOrEmpty(p.Token) && !p.AuthenticationStarted))
                {
                    player.AuthenticationStarted = true;
                    Task.Run(async () =>
                    {
                        using (var drc = new DiscordRestClient())
                        {
                            try
                            {
                                await drc.LoginAsync(Discord.TokenType.Bearer, player.Token);
                                if (drc.CurrentUser != null)
                                {
                                    var playerRoles = new List<string>();

                                    var userId = drc.CurrentUser.Id;

                                    await drc.LoginAsync(Discord.TokenType.Bot, World.GameConfiguration.DiscordToken);

                                    var user = await drc.GetGuildUserAsync(World.GameConfiguration.DiscordGuildID.Value, userId);
                                    var guild = await drc.GetGuildAsync(World.GameConfiguration.DiscordGuildID.Value);
                                    foreach (var roleID in user.RoleIds)
                                    {
                                        var role = guild.Roles.FirstOrDefault(r => r.Id == roleID);
                                        if (role != null)
                                            playerRoles.Add(role.Name);
                                    }

                                    player.LoginName = user.Nickname;
                                    player.LoginID = user.Id;
                                    player.Roles = playerRoles;
                                    player.OnAuthenticated();
                                }
                            }
                            catch (Exception) { }
                        }
                    });
                }
            }
        }
    }
}