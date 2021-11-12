namespace Game.Util.Commands
{
    using Game.API.Client;
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [Subcommand(typeof(Get))]
    [Subcommand(typeof(Reset))]
    [Subcommand(typeof(Stress))]
    [Subcommand(typeof(Announce))]
    [Subcommand(typeof(Connections))]
    [Command("server")]
    class ServerCommand : CommandBase
    {
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

        [Command("stress")]
        class Stress : CommandBase
        {
            [Argument(0)]
            public string World { get; set; }

            [Option]
            public bool ConnectionThrash { get; set; } = false;

            [Option]
            public bool SlowConsumer { get; set; } = false;

            [Option("--spectators")]
            public int Spectators { get; set; } = 0;

            protected async override Task ExecuteAsync()
            {
                ThreadPool.SetMinThreads(50, 50);

                var tasks = new List<Task>();
                if (ConnectionThrash)
                    tasks.Add(DoConnectionThrash());

                if (SlowConsumer)
                    tasks.Add(DoSlowConsumer());

                if (Spectators > 0)
                    tasks.Add(StartSpectators());

                await Task.WhenAll(tasks);
            }

            private Task StartSpectators()
            {
                var tasks = new List<Task>();
                for (int i = 0; i < Spectators; i++)
                    tasks.Add(this.Spectate());

                return Task.WhenAll(tasks);
            }

            private async Task Spectate()
            {
                var connection = new PlayerConnection(API.BaseURL.ToString(), this.World);

                connection.OnConnected = async () =>
                {
                    connection.ControlSpectate = "spectating";
                    await connection.SendControlInputAsync();
                    await connection.ListenAsync();
                };
                await connection.ConnectAsync();
            }

            private async Task DoSlowConsumer()
            {
                var connection = new PlayerConnection(API.BaseURL.ToString(), this.World);
                await connection.ConnectAsync();
                await connection.ListenAsync();
                connection.OnView = async () =>
                {
                    // start blocking
                    await Task.Delay(100000);
                };
            }

            private async Task DoConnectionThrash()
            {
                var connection = new PlayerConnection(API.BaseURL.ToString(), this.World);
                for (int i = 0; i < 1000; i++)
                {
                    Console.WriteLine($"Spawn #: {i + 1}");

                    await connection.ConnectAsync();
                    await connection.SpawnAsync("Testing", "ship_red", "red");
                    connection.Dispose();
                }
            }
        }

        [Command("announce")]
        class Announce : CommandBase
        {
            [Argument(0)]
            public string World { get; set; } = null;

            [Argument(1)]
            public string Message { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                if (Message != null)
                    await API.Server.AnnounceAsync(Message, World);
            }
        }

        [Command("connections")]
        class Connections : CommandBase
        {
            [Option]
            public bool IP { get; set; } = false;

            protected async override Task ExecuteAsync()
            {

                var worlds = await API.World.ListAsync();
                
                foreach (var world in worlds)
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
