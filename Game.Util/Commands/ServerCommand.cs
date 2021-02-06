namespace Game.Util.Commands
{
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [Subcommand(typeof(Get))]
    [Subcommand(typeof(Reset))]
    [Subcommand(typeof(Announce))]
    [Subcommand(typeof(Connections))]
    [Command("server")]
    class ServerCommand : CommandBase
    {
        private static string[] Worlds = new[]
        {
            "default",
            "duel",
            "team",
            "ctf"
        };

        private static string[] WorldSelection(string world)
        {
            if (world == null)
                return Worlds;
            else
                return new[] { world };
        }

        [Command("get")]
        class Get : CommandBase
        {
            protected async override Task ExecuteAsync()
            {
                var started = DateTime.Now;

                var server = await API.Server.ServerGetAsync();

                Table("Server", new
                {
                    Server = API.BaseURL.ToString(),
                    ms = DateTime.Now.Subtract(started).TotalMilliseconds,
                    Players = server.PlayerCount
                });
            }
        }

        [Command("reset")]
        class Reset : CommandBase
        {
            protected async override Task ExecuteAsync()
            {
                await API.Server.ServerResetAsync();
            }
        }

        [Command("announce")]
        class Announce : CommandBase
        {
            [Argument(0)]
            public string Message { get; set; } = null;

            [Option]
            public string World { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                if (Message != null)
                {
                    foreach (var world in WorldSelection(World))
                        await API.Server.AnnounceAsync(Message, world);
                }
            }
        }

        [Command("connections")]
        class Connections : CommandBase
        {
            [Option]
            public string World { get; set; } = null;

            [Option]
            public bool IP { get; set; } = false;

            protected async override Task ExecuteAsync()
            {
                foreach (var world in WorldSelection(World))
                {
                    var connections = await API.Server.ConnectionsAsync(world);

                    Table($"Connections - {world}", connections.Select(c =>
                        new
                        {
                            c.Name,
                            ip = IP ? c.IP : string.Empty,
                            c.IsAlive,
                            c.Score,
                            bg = c.Backgrounded,
                            c.Bandwidth,
                            fps = c.ClientFPS,
                            vps = c.ClientVPS,
                            ups = c.ClientUPS,
                            cs = c.ClientCS,
                            ping = c.Latency
                        }));
                }
            }
        }
    }
}
