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
                ProjectedBody followFleet = player?.Fleet;

                if (followFleet == null)
                    followFleet = Player.GetWorldPlayers(world)
                        .Where(p => p.IsAlive)
                        .OrderByDescending(p => p.Score * 10000 + (10000-p.Fleet.ID))
                        .FirstOrDefault()
                        ?.Fleet;

                if (followFleet == null)
                    followFleet = player?.World.Bodies.OfType<Ship>().FirstOrDefault();

                if (followFleet == null)
                    followFleet = player?.World.Bodies.FirstOrDefault();


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

                    var newHash = world.Hook.GetHashCode();

                    var builder = new FlatBufferBuilder(1);

                    // define camera
                    NetBody.StartNetBody(builder);
                    NetBody.AddDefinitionTime(builder, followFleet?.DefinitionTime ?? 0);
                    var momentum = followFleet?.Momentum ?? new Vector2(0, 0);
                    if (followFleet.Momentum != null)
                        NetBody.AddMomentum(builder, FromVector(builder, followFleet.Momentum));

                    if (followFleet.OriginalPosition != null)
                        NetBody.AddOriginalPosition(builder, FromVector(builder, followFleet.OriginalPosition));

                    var cameraBody = NetBody.EndNetBody(builder);

                    var updateVector = NetWorldView.CreateUpdatesVector(builder, updatedBuckets.Select(b =>
                    {
                        var s = b.BodyUpdated;
                        var c = b.BodyClient;

                        StringOffset stringSprite = new StringOffset();
                        StringOffset stringColor = new StringOffset();
                        StringOffset stringCaption = new StringOffset();

                        if (s.Sprite != c?.Sprite)
                            stringSprite = builder.CreateString(s.Sprite ?? string.Empty);
                        if (s.Color != c?.Color)
                            stringColor = builder.CreateString(s.Color ?? string.Empty);
                        if (s.Caption != c?.Caption)
                            stringCaption = builder.CreateString(s.Caption ?? string.Empty);

                        NetBody.StartNetBody(builder);
                        NetBody.AddId(builder, s.ID);
                        NetBody.AddDefinitionTime(builder, s.DefinitionTime);

                        if (s.Size != c?.Size)
                            NetBody.AddSize(builder, s.Size);

                        if (s.Sprite != c?.Sprite)
                            NetBody.AddSprite(builder, stringSprite);
                        if (s.Color != c?.Color)
                            NetBody.AddColor(builder, stringColor);
                        if (s.Caption != c?.Caption)
                            NetBody.AddCaption(builder, stringCaption);

                        if (s.OriginalAngle != c?.OriginalAngle)
                            NetBody.AddOriginalAngle(builder, s.OriginalAngle);

                        if (s.AngularVelocity != c?.AngularVelocity)
                            NetBody.AddAngularVelocity(builder, s.AngularVelocity);

                        NetBody.AddMomentum(builder, FromVector(builder, s.Momentum));
                        NetBody.AddOriginalPosition(builder, FromVector(builder, s.OriginalPosition));

                        return NetBody.EndNetBody(builder);
                    }).ToArray());

                    foreach (var update in updatedBuckets)
                        update.BodyClient = update.BodyUpdated.Clone();

                    var deletesVector = NetWorldView.CreateDeletesVector(builder, BodyCache.CollectStaleBuckets().Select(b =>
                        b.BodyUpdated.ID
                    ).ToArray());

                    NetWorldView.StartNetWorldView(builder);
                    NetWorldView.AddCamera(builder, cameraBody);
                    NetWorldView.AddIsAlive(builder, player?.IsAlive ?? false);
                    NetWorldView.AddTime(builder, world.Time);

                    NetWorldView.AddUpdates(builder, updateVector);

                    NetWorldView.AddDeletes(builder, deletesVector);

                    var worldView = NetWorldView.EndNetWorldView(builder);



                    // messages, hook, leaderboard

                    HookHash = newHash;

                    var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetWorldView, worldView.Value);
                    builder.Finish(q.Value);


                    // if(updates.Any())
                    await this.SendAsync(builder.DataBuffer, cancellationToken);
                }

                if (LeaderboardTime != (world.Leaderboard?.Time ?? 0))
                {
                    LeaderboardTime = (world.Leaderboard?.Time ?? 0);

                    var builder = new FlatBufferBuilder(1);

                    var stringName = builder.CreateString(world.Leaderboard.ArenaRecord.Name ?? " ");
                    var stringColor = builder.CreateString(world.Leaderboard.ArenaRecord.Color ?? " ");

                    NetLeaderboardEntry.StartNetLeaderboardEntry(builder);
                    NetLeaderboardEntry.AddColor(builder, stringColor);
                    NetLeaderboardEntry.AddName(builder, stringName);
                    NetLeaderboardEntry.AddScore(builder, world.Leaderboard.ArenaRecord.Score);
                    var record = NetLeaderboardEntry.EndNetLeaderboardEntry(builder);

                    
                    var entriesVector = NetLeaderboard.CreateEntriesVector(builder, world.Leaderboard.Entries.Select(e =>
                    {
                        stringName = builder.CreateString(e.Name ?? string.Empty);
                        stringColor = builder.CreateString(e.Color ?? string.Empty);

                        NetLeaderboardEntry.StartNetLeaderboardEntry(builder);
                        NetLeaderboardEntry.AddName(builder, stringName);
                        NetLeaderboardEntry.AddColor(builder, stringColor);
                        NetLeaderboardEntry.AddScore(builder, e.Score);
                        NetLeaderboardEntry.AddPosition(builder, FromVector(builder, e.Position));

                        return NetLeaderboardEntry.EndNetLeaderboardEntry(builder);
                    }).ToArray());

                    var stringType = builder.CreateString(world.Leaderboard.Type ?? string.Empty);
                    NetLeaderboard.StartNetLeaderboard(builder);
                    NetLeaderboard.AddEntries(builder, entriesVector);
                    NetLeaderboard.AddType(builder, stringType);
                    NetLeaderboard.AddRecord(builder, record);

                    var leaderboardOffset = NetLeaderboard.EndNetLeaderboard(builder);

                    builder.Finish(NetQuantum.CreateNetQuantum(builder, AllMessages.NetLeaderboard, leaderboardOffset.Value).Value);
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

        private async Task HandleIncomingMessage(NetQuantum quantum)
        {
            switch (quantum.MessageType)
            {
                case AllMessages.NetPing:
                    var ping = quantum.Message<NetPing>().Value;
                    var builder = new FlatBufferBuilder(1);
                    var pong = NetPing.CreateNetPing(builder, world.Time);
                    var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetPing, pong.Value);
                    builder.Finish(q.Value);

                    await SendAsync(builder.DataBuffer, default(CancellationToken));
                    break;

                case AllMessages.NetSpawn:
                    var spawn = quantum.Message<NetSpawn>().Value;

                    player.Spawn(spawn.Name, spawn.Ship, spawn.Color);

                    break;
                case AllMessages.NetControlInput:
                    var input = quantum.Message<NetControlInput>().Value;
                    player?.SetControl(new ControlInput
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
            var ping = NetPing.CreateNetPing(builder, world.Time);
            builder.Finish(ping.Value);

            await this.SendAsync(builder.DataBuffer, cancellationToken);

            ConnectionHeartbeat.Register(this);

            try
            {
                lock (world.Bodies)
                {
                    player = new Player();
                    player.Init(world);
                }

                await StartReadAsync(this.HandleIncomingMessage, cancellationToken);
            }
            finally
            {
                ConnectionHeartbeat.Unregister(this);

                if (player != null)
                    player.Deinit();
            }
        }

        private async Task<bool> StartReadAsync(Func<NetQuantum, Task> onReceive, CancellationToken cancellationToken = default(CancellationToken))
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
                                var quantum = NetQuantum.GetRootAsNetQuantum(dataBuffer);

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