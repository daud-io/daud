namespace Game.Engine.Core
{
    using Discord.Rest;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Authenticator : IActor
    {
        private World World = null;

        public void CreateDestroy()
        {
        }

        public void Destroy()
        {
            this.World.Actors.Remove(this);
        }

        public void Init(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);
        }

        public void Think()
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

                                var user = await drc.GetGuildUserAsync(472025150959648791, userId);
                                var guild = await drc.GetGuildAsync(472025150959648791);
                                foreach (var roleID in user.RoleIds)
                                {
                                    var role = guild.Roles.FirstOrDefault(r => r.Id == roleID);
                                    if (role != null)
                                        playerRoles.Add(role.Name);
                                }

                                player.Roles = playerRoles;
                            }
                        }
                        catch (Exception) { }
                    }
                });
            }
        }
    }
}