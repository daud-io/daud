namespace Game.Engine.Networking
{
    using Game.Engine.Core;
    using Game.Models;
    using Game.Models.Messages;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Numerics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class Connection : IDisposable
    {
        private static readonly List<Connection> connections = new List<Connection>();

        private readonly ILogger<Connection> Logger = null;
        private readonly SemaphoreSlim WebsocketSendingSemaphore = new SemaphoreSlim(1, 1);
        private readonly BodyCache BodyCache = new BodyCache();

        private WebSocket Socket = null;

        private World world = null;
        private Player player = null;

        private int HookHash = 0;
        private long LeaderboardTime = 0;

        public Connection(ILogger<Connection> logger)
        {
            this.Logger = logger;
        }

        public async Task StepAsync(CancellationToken cancellationToken)
        {
            if (player != null)
            {
                var followFleet = player?.Fleet;

                if (followFleet == null)
                    followFleet = Player.GetWorldPlayers(world)
                        .Where(p => p.IsAlive)
                        .OrderByDescending(p => p.Score)
                        .FirstOrDefault()
                        ?.Fleet;

                IEnumerable<ProjectedBody> updatedBodies = null;

                if (followFleet != null)
                {
                    var halfViewport = new Vector2(3000, 3000);

                    var updates = BodyCache.Update(
                        world.Bodies,
                        world.Time,
                        Vector2.Subtract(followFleet.Position, halfViewport),
                        Vector2.Add(followFleet.Position, halfViewport)
                    );

                    var updatedBuckets = updates.Take(30);
                    //var updatedBuckets = updates;

                    foreach (var update in updatedBuckets)
                    {
                        update.BodyClient = update.BodyUpdated.Clone();
                    }

                    updatedBodies = updatedBuckets.Select(b => b.BodyClient);

                    var newHash = world.Hook.GetHashCode();

                    var playerView = new PlayerView
                    {
                        Time = world.Time,
                        PlayerCount = 1,

                        Updates = updatedBodies.ToList(),
                        Deletes = BodyCache.CollectStaleBuckets().Select(b => b.BodyUpdated.ID),

                        DefinitionTime = followFleet?.DefinitionTime ?? 0,
                        OriginalPosition = followFleet?.OriginalPosition ?? new Vector2(0, 0),
                        Momentum = followFleet?.Momentum ?? new Vector2(0, 0),
                        IsAlive = player?.IsAlive ?? false,
                        Messages = player?.GetMessages(),
                        Hook = HookHash != newHash
                            ? world.Hook
                            : null,
                        Leaderboard = LeaderboardTime != (world.Leaderboard?.Time ?? 0)
                            ? world.Leaderboard
                            : null
                    };
                    HookHash = newHash;
                    LeaderboardTime = (world.Leaderboard?.Time ?? 0);

                    var view = new View
                    {
                        PlayerView = playerView
                    };

                    if (playerView.Updates.Any() || playerView.Deletes.Any())
                        await this.SendAsync(view, cancellationToken);
                }
            }
        }

        private async Task SendAsync(MessageBase message, CancellationToken cancellationToken)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);

            await SendAsync(buffer, cancellationToken);
        }

        public async Task SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            await WebsocketSendingSemaphore.WaitAsync();
            try
            {
                var start = DateTime.Now;

                await Socket.SendAsync(
                    buffer, 
                    WebSocketMessageType.Text, 
                    endOfMessage: true, 
                    cancellationToken: cancellationToken);

                //Console.WriteLine($"{DateTime.Now.Subtract(start).TotalMilliseconds}ms in send");
            }
            finally
            {
                WebsocketSendingSemaphore.Release();
            }
        }

        private async Task HandleIncomingMessage(MessageBase message)
        {
            if (message is Ping)
            {
                await SendAsync(message, default(CancellationToken));
            }
            else if (message is Spawn)
            {
                var s = message as Spawn;

                if (player == null)
                {
                    lock (world.Bodies)
                    {
                        player = new Player();
                        player.Init(world);
                    }
                }

                player.Spawn();
            }
            else if (message is ControlInput)
            {
                var s = message as ControlInput;
                if (player != null)
                    player.SetControl(s);
            }
            else if (message is Hook)
            {
                var hook = message as Hook;
                world.Hook = hook;  
            }
        }

        public async Task ConnectAsync(HttpContext httpContext, WebSocket socket, CancellationToken cancellationToken = default(CancellationToken))
        {
            Socket = socket;

            world = Worlds.Find();

            await this.SendAsync(new Hello(), cancellationToken);

            ConnectionHeartbeat.Register(this);

            try
            {
                await StartReadAsync(this.HandleIncomingMessage, cancellationToken);
            }
            finally
            {
                ConnectionHeartbeat.Unregister(this);

                if (player != null)
                    player.Deinit();
            }
        }

        private async Task<bool> StartReadAsync(Func<MessageBase, Task> onReceive, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = new WebSocketReceiveResult(0, WebSocketMessageType.Text, false);

                while (!result.CloseStatus.HasValue && Socket.State == WebSocketState.Open)
                {
                    int maxlength = 1024 * 1024 * 1;
                    using (var ms = new MemoryStream())
                    {
                        while (!result.EndOfMessage && !Socket.CloseStatus.HasValue && ms.Length < maxlength)
                        {
                            result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                            ms.Write(buffer, 0, result.Count);
                        }

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", cancellationToken);
                        }

                        if (!result.CloseStatus.HasValue)
                        {
                            if (result.EndOfMessage)
                            {
                                var bytes = ms.GetBuffer();
                                var json = Encoding.UTF8.GetString(bytes, 0, (int)ms.Length);
                                var item = JObject.Parse(json);

                                var enumType = (MessageBase.MessageTypes)item["Type"].Value<int>();
                                var type = MessageBase.MessageTypeMap[enumType];

                                MessageBase message = item.ToObject(type) as MessageBase;

                                await onReceive(message);

                                result = new WebSocketReceiveResult(0, WebSocketMessageType.Text, false);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void Close()
        {
            try
            {
                Socket.Abort();
            }
            catch (Exception) { }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Socket != null)
                        Socket.Dispose();
                }
                disposedValue = true;
            }
        }
        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}