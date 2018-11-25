namespace Game.Engine.Networking
{
    using Game.Engine.Core;
    using Game.Engine.Networking.FlatBuffers;
    using Google.FlatBuffers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Nito.AsyncEx;
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

        public AsyncAutoResetEvent WorldUpdateEvent = null;

        public Connection(ILogger<Connection> logger)
        {
            this.Logger = logger;
            WorldUpdateEvent = new AsyncAutoResetEvent();
        }

        public async Task StartSynchronizing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await WorldUpdateEvent.WaitAsync(cancellationToken);
                await StepAsync(cancellationToken);
            }
        }

        private Offset<Vec2> FromPositionVector(FlatBufferBuilder builder, Vector2 vector)
        {
            return Vec2.CreateVec2(builder, (short)vector.X, (short)vector.Y);
        }

        public async Task StepAsync(CancellationToken cancellationToken)
        {
            if (player != null)
            {
                // default to player
                var followFleet = player?.Fleet;

                // if the player doesn't have a fleet alive
                if (followFleet == null)
                {
                    // find someone else to watch
                    followFleet = Player.GetWorldPlayers(world)
                        .ToList()
                        .Where(p => p.IsAlive)
                        .OrderByDescending(p => p.Score * 10000 + (10000 - p.Fleet.ID))
                        .FirstOrDefault()
                        ?.Fleet;
                }

                Body followBody = null;

                // if we're watching a fleet, watch the center of their fleet
                if (followFleet != null)
                {
                    var center = Core.Steering.Flocking.FleetCenterNaive(followFleet.Ships);

                    followBody = new Body
                    {
                        DefinitionTime = world.Time,
                        OriginalPosition = center,
                        Position = center,
                        Momentum = followFleet.FleetMomentum
                    };
                }

                // if we haven't found anything to watch yet, watch the first ship we find
                if (followBody == null)
                {
                    followBody = player?.World.Bodies.OfType<Ship>().FirstOrDefault();
                }

                // if we haven't found anything to watch yet, watch anything
                if (followBody == null)
                {
                    followBody = player?.World.Bodies.FirstOrDefault();
                }

                if (followBody != null)
                {
                    var halfViewport = new Vector2(3000, 3000);

                    BodyCache.Update(
                        world.Bodies,
                        world.Groups,
                        world.Time,
                        Vector2.Subtract(followBody.Position, halfViewport),
                        Vector2.Add(followBody.Position, halfViewport)
                    );

                    var updates = BodyCache.BodiesByError();

                    var updateBodies = updates.Take(100);

                    var newHash = world.Hook.GetHashCode();

                    var builder = new FlatBufferBuilder(1);
                    float VELOCITY_SCALE_FACTOR = 10000f;

                    var updatedGroups = BodyCache.GroupsByError().ToList();

                    var groupsVector = NetWorldView.CreateGroupsVector(builder,
                        updatedGroups.Select(b =>
                        {
                            var serverGroup = b.GroupUpdated;

                            var caption = builder.CreateString(serverGroup.Caption ?? " ");

                            var group = NetGroup.CreateNetGroup(builder,
                                group: serverGroup.ID,
                                type: serverGroup.GroupType,
                                captionOffset: caption,
                                zindex: serverGroup.ZIndex
                            );
                            return group;
                        }).ToArray());


                    foreach (var update in updatedGroups)
                    {
                        update.GroupClient = update.GroupUpdated.Clone();
                    }

                    var groupDeletesVector = NetWorldView.CreateGroupDeletesVector(builder, BodyCache.CollectStaleGroups().Select(b =>
                        b.GroupUpdated.ID
                    ).ToArray());

                    NetWorldView.StartUpdatesVector(builder, updateBodies.Count());
                    foreach (var b in updateBodies)
                    {
                        var serverBody = b.BodyUpdated;

                        var body = NetBody.CreateNetBody(builder,
                            Id: serverBody.ID,
                            DefinitionTime: serverBody.DefinitionTime,
                            originalPosition_X: (short)serverBody.OriginalPosition.X,
                            originalPosition_Y: (short)serverBody.OriginalPosition.Y,
                            velocity_X: (short)(serverBody.Momentum.X * VELOCITY_SCALE_FACTOR),
                            velocity_Y: (short)(serverBody.Momentum.Y * VELOCITY_SCALE_FACTOR),
                            OriginalAngle: (sbyte)(serverBody.OriginalAngle / MathF.PI * 127),
                            AngularVelocity: (sbyte)(serverBody.AngularVelocity * VELOCITY_SCALE_FACTOR / MathF.PI * 127),
                            Size: (byte)(serverBody.Size / 5),
                            Sprite: (byte)serverBody.Sprite,
                            Mode: 0,
                            Group: serverBody.Group?.ID ?? 0);
                    }

                    var updatesVector = builder.EndVector();

                    foreach (var update in updateBodies)
                    {
                        update.BodyClient = update.BodyUpdated.Clone();
                    }

                    var deletesVector = NetWorldView.CreateDeletesVector(builder, BodyCache.CollectStaleBuckets().Select(b =>
                        b.BodyUpdated.ID
                    ).ToArray());

                    var messages = player.GetMessages();
                    VectorOffset announcementsVector = new VectorOffset();
                    if (messages != null && messages.Any())
                    {
                        announcementsVector = NetWorldView.CreateAnnouncementsVector(builder, messages.Select(e =>
                        {
                            var stringName = builder.CreateString(e);

                            NetAnnouncement.StartNetAnnouncement(builder);
                            NetAnnouncement.AddText(builder, stringName);

                            return NetAnnouncement.EndNetAnnouncement(builder);
                        }).ToArray());
                    }

                    NetWorldView.StartNetWorldView(builder);

                    // define camera
                    var cameraBody = NetBody.CreateNetBody(
                        builder,
                        Id: 0,
                        DefinitionTime: followBody?.DefinitionTime ?? 0,
                        originalPosition_X: (short)(followBody?.OriginalPosition.X ?? 0),
                        originalPosition_Y: (short)(followBody?.OriginalPosition.Y ?? 0),
                        velocity_X: (short)(followBody?.Momentum.X * VELOCITY_SCALE_FACTOR ?? 0),
                        velocity_Y: (short)(followBody?.Momentum.Y * VELOCITY_SCALE_FACTOR ?? 0),
                        OriginalAngle: (sbyte)(followBody?.OriginalAngle / MathF.PI / 127 ?? 0),
                        AngularVelocity: 0,
                        Size: 0,
                        Sprite: 0,
                        Mode: 0,
                        Group: 0
                    );

                    NetWorldView.AddCamera(builder, cameraBody);
                    NetWorldView.AddIsAlive(builder, player?.IsAlive ?? false);
                    NetWorldView.AddTime(builder, world.Time);

                    NetWorldView.AddUpdates(builder, updatesVector);
                    NetWorldView.AddDeletes(builder, deletesVector);

                    NetWorldView.AddGroups(builder, groupsVector);
                    NetWorldView.AddGroupDeletes(builder, groupDeletesVector);
                    if (messages != null && messages.Any())
                        NetWorldView.AddAnnouncements(builder, announcementsVector);

                    var worldView = NetWorldView.EndNetWorldView(builder);

                    HookHash = newHash;

                    var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetWorldView, worldView.Value);
                    builder.Finish(q.Value);

                    await this.SendAsync(builder.DataBuffer, cancellationToken);
                }

                if (LeaderboardTime != (world.Leaderboard?.Time ?? 0))
                {
                    LeaderboardTime = (world.Leaderboard?.Time ?? 0);

                    var builder = new FlatBufferBuilder(1);

                    var stringName = builder.CreateString(world.Leaderboard?.ArenaRecord?.Name ?? " ");
                    var stringColor = builder.CreateString(world.Leaderboard?.ArenaRecord?.Color ?? " ");
                    var stringToken = builder.CreateString(world.Leaderboard?.ArenaRecord?.Token ?? "");

                    NetLeaderboardEntry.StartNetLeaderboardEntry(builder);
                    NetLeaderboardEntry.AddColor(builder, stringColor);
                    NetLeaderboardEntry.AddName(builder, stringName);
                    NetLeaderboardEntry.AddScore(builder, world.Leaderboard?.ArenaRecord?.Score ?? 0);
                    NetLeaderboardEntry.AddToken(builder, stringToken);
                    var record = NetLeaderboardEntry.EndNetLeaderboardEntry(builder);


                    var entriesVector = NetLeaderboard.CreateEntriesVector(builder, world.Leaderboard.Entries.Select(e =>
                    {
                        stringName = builder.CreateString(e.Name ?? string.Empty);
                        stringColor = builder.CreateString(e.Color ?? string.Empty);
                        stringToken = builder.CreateString(e.Token ?? string.Empty);

                        NetLeaderboardEntry.StartNetLeaderboardEntry(builder);
                        NetLeaderboardEntry.AddName(builder, stringName);
                        NetLeaderboardEntry.AddColor(builder, stringColor);
                        NetLeaderboardEntry.AddScore(builder, e.Score);
                        NetLeaderboardEntry.AddPosition(builder, FromPositionVector(builder, e.Position));
                        NetLeaderboardEntry.AddToken(builder, stringToken);

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

        private async Task SendPingAsync()
        {
            var builder = new FlatBufferBuilder(1);
            var pong = NetPing.CreateNetPing(builder, world.Time);
            var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetPing, pong.Value);
            builder.Finish(q.Value);

            await SendAsync(builder.DataBuffer, default(CancellationToken));
        }

        private async Task HandleIncomingMessage(NetQuantum quantum)
        {
            switch (quantum.MessageType)
            {
                case AllMessages.NetPing:
                    await SendPingAsync();
                    break;

                case AllMessages.NetSpawn:
                    var spawn = quantum.Message<NetSpawn>().Value;

                    Sprites shipSprite = Sprites.ship_red;

                    switch (spawn.Color)
                    {
                        case "ship0":
                            shipSprite = Sprites.ship0;
                            break;
                        case "green":
                            shipSprite = Sprites.ship_green;
                            break;
                        case "orange":
                            shipSprite = Sprites.ship_orange;
                            break;
                        case "pink":
                            shipSprite = Sprites.ship_pink;
                            break;
                        case "red":
                            shipSprite = Sprites.ship_red;
                            break;
                        case "cyan":
                            shipSprite = Sprites.ship_cyan;
                            break;
                        case "yellow":
                            shipSprite = Sprites.ship_yellow;
                            break;
                    }

                    player.Spawn(spawn.Name, shipSprite, spawn.Color, spawn.Token);

                    break;
                case AllMessages.NetControlInput:
                    var input = quantum.Message<NetControlInput>().Value;
                    player?.SetControl(new ControlInput
                    {
                        Position = new Vector2(input.X, input.Y),
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
            await SendPingAsync();

            ConnectionHeartbeat.Register(this);

            try
            {
                lock (world.Bodies)
                {
                    player = new Player
                    {
                        IP = httpContext.Connection.RemoteIpAddress.ToString()
                    };
                    player.Init(world);
                }

                var updateTask = StartSynchronizing(cancellationToken);
                var readTask = StartReadAsync(this.HandleIncomingMessage, cancellationToken);

                await Task.WhenAny(updateTask, readTask);

            }
            finally
            {
                ConnectionHeartbeat.Unregister(this);

                if (player != null)
                {
                    player.PendingDestruction = true;
                }
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
                    {
                        Socket.Dispose();
                    }
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