namespace Game.Engine.Networking
{
    using Game.Engine.Core;
    using Game.Engine.Networking.FlatBuffers;
    using Google.FlatBuffers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Numerics;
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

        private Offset<Vec2> FromVector(FlatBufferBuilder builder, Vector2 vector)
        {
            return Vec2.CreateVec2(builder, vector.X, vector.Y);
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

                ProjectedBody[] updatedBodies = null;

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

                    updatedBodies = updatedBuckets.Select(b => b.BodyClient).ToArray();

                    var newHash = world.Hook.GetHashCode();

                    var builder = new FlatBufferBuilder(1);

                    // define camera
                    PhysicalBody.StartPhysicalBody(builder);
                    PhysicalBody.AddDefinitionTime(builder, followFleet?.DefinitionTime ?? 0);
                    var momentum = followFleet?.Momentum ?? new Vector2(0, 0);
                    if (followFleet.Momentum != null)
                        PhysicalBody.AddMomentum(builder, FromVector(builder, followFleet.Momentum));

                    if (followFleet.OriginalPosition != null)
                        PhysicalBody.AddOriginalPosition(builder, FromVector(builder, followFleet.OriginalPosition));

                    var cameraBody = PhysicalBody.EndPhysicalBody(builder);

                    var updateVector = WorldView.CreateUpdatesVector(builder, updatedBodies.Select(u =>
                    {

                        var stringSprite = builder.CreateString(u.Sprite ?? string.Empty);
                        var stringColor = builder.CreateString(u.Color ?? string.Empty);
                        var stringCaption = builder.CreateString(u.Caption ?? string.Empty);

                        PhysicalBody.StartPhysicalBody(builder);
                        PhysicalBody.AddId(builder, u.ID);
                        PhysicalBody.AddDefinitionTime(builder, u.DefinitionTime);
                        PhysicalBody.AddSize(builder, u.Size);
                        PhysicalBody.AddSprite(builder, stringSprite);
                        PhysicalBody.AddColor(builder, stringColor);
                        PhysicalBody.AddCaption(builder, stringCaption);
                        PhysicalBody.AddAngle(builder, u.Angle);
                        PhysicalBody.AddMomentum(builder, FromVector(builder, u.Momentum));
                        PhysicalBody.AddOriginalPosition(builder, FromVector(builder, u.OriginalPosition));

                        return PhysicalBody.EndPhysicalBody(builder);
                    }).ToArray());

                    var deletesVector = WorldView.CreateDeletesVector(builder, BodyCache.CollectStaleBuckets().Select(b =>
                        b.BodyUpdated.ID
                    ).ToArray());

                    WorldView.StartWorldView(builder);
                    WorldView.AddCamera(builder, cameraBody);
                    WorldView.AddIsAlive(builder, player?.IsAlive ?? false);
                    WorldView.AddTime(builder, world.Time);

                    WorldView.AddUpdates(builder, updateVector);

                    WorldView.AddDeletes(builder, deletesVector);

                    var worldView = WorldView.EndWorldView(builder);

                    // messages, hook, leaderboard

                    HookHash = newHash;
                    LeaderboardTime = (world.Leaderboard?.Time ?? 0);

                    var q = Quantum.CreateQuantum(builder, AllMessages.WorldView, worldView.Value);
                    builder.Finish(q.Value);


                    // if(updates.Any())
                    await this.SendAsync(builder.DataBuffer, cancellationToken);
                }
            }
        }

        private async Task SendAsync(ByteBuffer message, CancellationToken cancellationToken)
        {
            var buffer = message.ToSizedArray();

            await WebsocketSendingSemaphore.WaitAsync();
            try
            {
                var start = DateTime.Now;

                await Socket.SendAsync(
                    buffer, 
                    WebSocketMessageType.Binary, 
                    endOfMessage: true, 
                    cancellationToken: cancellationToken);

                //Console.WriteLine($"{DateTime.Now.Subtract(start).TotalMilliseconds}ms in send");
            }
            finally
            {
                WebsocketSendingSemaphore.Release();
            }
        }

        private async Task HandleIncomingMessage(Quantum quantum)
        {
            switch (quantum.MessageType)
            {
                case AllMessages.Ping:
                    var ping = quantum.Message<FlatBuffers.Ping>().Value;
                    var builder = new FlatBufferBuilder(1);
                    var pong = FlatBuffers.Ping.CreatePing(builder, world.Time);
                    var q = Quantum.CreateQuantum(builder, AllMessages.Ping, pong.Value);
                    builder.Finish(q.Value);

                    await SendAsync(builder.DataBuffer, default(CancellationToken));
                    break;

                case AllMessages.Spawn:
                    var spawn = quantum.Message<FlatBuffers.Spawn>().Value;

                    if (player == null)
                    {
                        lock (world.Bodies)
                        {
                            player = new Player();
                            player.Init(world);
                        }
                    }

                    player.Spawn(spawn.Name, spawn.Ship, spawn.Color);

                    break;
                case AllMessages.ControlInput:
                    var input = quantum.Message<FlatBuffers.ControlInput>().Value;
                    player?.SetControl(new Game.Engine.Core.ControlInput
                    {
                        Angle = input.Angle,
                        BoostRequested = input.Boost,
                        ShootRequested = input.Shoot
                    });
                    break;
            }

            /*
            else if (message is Hook)
            {
                var hook = message as Hook;
                world.Hook = hook;  
            }*/
        }

        public async Task ConnectAsync(HttpContext httpContext, WebSocket socket, CancellationToken cancellationToken = default(CancellationToken))
        {
            Socket = socket;

            world = Worlds.Find();

            var builder = new FlatBufferBuilder(1);
            var ping = FlatBuffers.Ping.CreatePing(builder, world.Time);
            builder.Finish(ping.Value);

            await this.SendAsync(builder.DataBuffer, cancellationToken);

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

        private async Task<bool> StartReadAsync(Func<Quantum, Task> onReceive, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = new WebSocketReceiveResult(0, WebSocketMessageType.Binary, false);

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
                                var dataBuffer = new ByteBuffer(bytes);
                                var quantum = Quantum.GetRootAsQuantum(dataBuffer);

                                await onReceive(quantum);

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