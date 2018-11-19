namespace Game.Engine.Controllers
{
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Game.Engine.Core;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;

    public class ServerController : APIControllerBase
    {
        public ServerController(ISecurityContext securityContext)
             : base(securityContext)
        {
        }

        [HttpGet, AllowAnonymous]
        public Server Get()
        {
            var world = Worlds.Find();

            return new Server
            {
                PlayerCount = Player.GetWorldPlayers(world).Count,
                WorldCount = 1
            };
        }

        [HttpPost, Route("reset")]
        public bool Reset()
        {
            Program.Abort();

            return true;
        }

        [HttpPost, Route("announce")]
        public bool Announce(string message)
        {
            var world = Worlds.Find();
            var players = Player.GetWorldPlayers(world);
            foreach (var player in players)
                player.SendMessage(message);

            return true;
        }

        [HttpGet, Route("players")]
        public IEnumerable<GameConnection> GetPlayers()
        {
            var world = Worlds.Find();
            return Player.GetWorldPlayers(world)
                .Select(p => new GameConnection
                {
                    Name = p.Name,
                    Score = p.Score,
                    IsAlive = p.IsAlive,
                    IP = p.IP
                });
        }
    }
}
