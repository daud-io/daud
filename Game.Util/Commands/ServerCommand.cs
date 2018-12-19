namespace Game.Util.Commands
{
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [Subcommand("get", typeof(Get))]
    [Subcommand("reset", typeof(Reset))]
    [Subcommand("announce", typeof(Announce))]
    [Subcommand("connections", typeof(Connections))]
    [Subcommand("hook", typeof(Hook))]
    [Subcommand("world", typeof(World))]
    class ServerCommand : CommandBase
    {
        private static string[] Worlds = new[]
        {
            "default",
            "other",
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

        class Reset : CommandBase
        {
            protected async override Task ExecuteAsync()
            {
                await API.Server.ServerResetAsync();
            }
        }

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

        class Hook : CommandBase
        {
            [Option]
            public string World { get; set; } = null;

            [Argument(0)]
            public string HookJSON { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                var hook = JsonConvert.DeserializeObject(HookJSON);
                hook = await API.Server.HookAsync(hook, World);

                Console.WriteLine(hook);
            }
        }

        [Subcommand("shrink", typeof(Shrink))]
        class World : CommandBase
        {
            class Shrink : CommandBase
            {
                [Option]
                public string World { get; set; } = null;

                protected async override Task ExecuteAsync()
                {
                    for (int i = 4200; i > 0; i -= 10)
                    {
                        await API.Server.HookAsync(new { WorldSize = i }, World);
                        await Task.Delay(100);
                    }
                }
            }
        }
    }
}
