using Newtonsoft.Json;

namespace Game.Engine.Networking
{
    using Game.API.Common;
    using Game.Engine.Core;
    using Game.Engine.Core.Steering;
    using global::FlatBuffers;
    using Game.Engine.Networking.FlatBuffers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Nito.AsyncEx;
    using RBush;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Numerics;
    using System.Text.RegularExpressions;
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

        public bool Backgrounded { get; set; } = false;
        public uint ClientFPS { get; set; } = 0;
        public uint ClientVPS { get; set; } = 0;
        public uint ClientUPS { get; set; } = 0;
        public uint ClientCS { get; set; } = 0;
        public uint Bandwidth { get; set; } = 100;
        public uint Latency { get; set; } = 0;

        public bool IsSpectating { get; set; } = false;

        public Fleet SpectatingFleet = null;

        public string CustomData = null;

        public Queue<BroadcastEvent> Events = new Queue<BroadcastEvent>();

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
            try
            {

                if (player != null)
                {
                    var builder = new FlatBufferBuilder(1);

                    lock (world.Bodies) // wrong kind of lock but might do for now
                    {
                        // First try to focus camera on the player if they have
                        // a fleet alive;
                        var followFleet = player?.Fleet;

                        // if the player doesn't have a fleet alive
                        if (followFleet == null)
                            // check to see if they are spectating a fleet that's alive
                            if (SpectatingFleet != null && SpectatingFleet.Exists)
                                followFleet = SpectatingFleet;

                        if (followFleet == null)
                            // find someone else to watch
                            followFleet = Player.GetWorldPlayers(world)
                                .ToList()
                                .Where(p => p.IsAlive)
                                .OrderByDescending(p => p.Score * 10000 + (10000 - p.Fleet?.ID ?? 0))
                                .FirstOrDefault()
                                ?.Fleet;

                        Body followBody = null;

                        // if we're watching a fleet, watch the center of their fleet
                        if (followFleet != null)
                        {
                            if (world.Hook.FollowFirstShip)
                                followBody = followFleet.Ships.FirstOrDefault();
                            else
                            {
                                var center = FleetMath.FleetCenterNaive(followFleet.Ships);
                                followBody = new Body
                                {
                                    DefinitionTime = world.Time,
                                    OriginalPosition = center,
                                    Position = center,
                                    Momentum = followFleet.FleetMomentum
                                };
                            }
                        }

                        // we've found someone to spectate, record it
                        if (followFleet != player?.Fleet && followFleet != SpectatingFleet)
                            SpectatingFleet = followFleet;

                        // if we haven't found anything to watch yet, watch the first ship we find
                        if (followBody == null)
                            followBody = player?.World.Bodies.OfType<Ship>().FirstOrDefault();

                        // if we haven't found anything to watch yet, watch anything
                        if (followBody == null)
                            followBody = player?.World.Bodies.FirstOrDefault();

                        if (followBody != null)
                        {
                            var size = 6000;
                            var viewportHeight = size * 2;
                            var viewportWidth = size * 2;

                            var playerViewport = new Envelope(
                                followBody.Position.X - viewportWidth / 2,
                                followBody.Position.Y - viewportHeight / 2,
                                followBody.Position.X + viewportWidth / 2,
                                followBody.Position.Y + viewportHeight / 2
                            );

                            BodyCache.Update(
                                world.BodiesNear(playerViewport).ToList(),
                                world.Time
                            );

                            var updates = BodyCache.BodiesByError();

                            var updateBodies = updates.Take((int)this.Bandwidth);

                            float VELOCITY_SCALE_FACTOR = 5000;

                            var updatedGroups = BodyCache.GroupsByError().ToList();

                            var groupsVector = NetWorldView.CreateGroupsVector(builder,
                                updatedGroups.Select(b =>
                                {
                                    var serverGroup = b.GroupUpdated;

                                    var caption = builder.CreateString(serverGroup.Caption ?? " ");
                                    var color = builder.CreateString(serverGroup.Color ?? "");
                                    var customData = builder.CreateString(serverGroup.CustomData ?? "");

                                    var group = NetGroup.CreateNetGroup(builder,
                                        group: serverGroup.ID,
                                        type: (byte)serverGroup.GroupType,
                                        captionOffset: caption,
                                        zindex: serverGroup.ZIndex,
                                        owner: serverGroup.OwnerID,
                                        colorOffset: color,
                                        customDataOffset: customData
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
                                    AngularVelocity: (sbyte)(serverBody.AngularVelocity * 10000),
                                    Size: (byte)(serverBody.Size / 5),
                                    Sprite: (ushort)serverBody.Sprite,
                                    Mode: serverBody.Mode,
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
                                    var stringType = builder.CreateString(e.Type);
                                    var stringMessage = builder.CreateString(e.Message);
                                    var stringExtraData = e.ExtraData != null
                                        ? builder.CreateString(JsonConvert.SerializeObject(e.ExtraData))
                                        : new StringOffset();

                                    NetAnnouncement.StartNetAnnouncement(builder);
                                    NetAnnouncement.AddType(builder, stringType);
                                    NetAnnouncement.AddText(builder, stringMessage);
                                    if (e.ExtraData != null)
                                        NetAnnouncement.AddExtraData(builder, stringExtraData);
                                    NetAnnouncement.AddPointsDelta(builder, e.PointsDelta);

                                    return NetAnnouncement.EndNetAnnouncement(builder);
                                }).ToArray());
                            }

                            StringOffset customOffset = new StringOffset();
                            if (followFleet?.CustomData != null && followFleet.CustomData != CustomData)
                                customOffset = builder.CreateString(followFleet.CustomData);

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

                            if (followFleet?.CustomData != null && followFleet.CustomData != CustomData)
                                NetWorldView.AddCustomData(builder, customOffset);
                            CustomData = followFleet?.CustomData;

                            var players = Player.GetWorldPlayers(world);
                            NetWorldView.AddPlayerCount(builder, (uint)world.AdvertisedPlayerCount);
                            NetWorldView.AddSpectatorCount(builder, (uint)world.SpectatorCount);

                            NetWorldView.AddCooldownBoost(builder, (byte)((player?.Fleet?.BoostCooldownStatus * 255) ?? 0));
                            NetWorldView.AddCooldownShoot(builder, (byte)((player?.Fleet?.ShootCooldownStatus * 255) ?? 0));
                            NetWorldView.AddWorldSize(builder, (ushort)world.Hook.WorldSize);

                            if (followFleet != null)
                                // inform the client of which the fleet id
                                NetWorldView.AddFleetID(builder, (uint)followFleet.ID);
                            else
                                NetWorldView.AddFleetID(builder, 0);

                            var worldView = NetWorldView.EndNetWorldView(builder);

                            var newHash = world.Hook.GetHashCode();
                            if (HookHash != newHash)
                            {
                                this.Events.Enqueue(new BroadcastEvent
                                {
                                    EventType = "hook",
                                    Data = JsonConvert.SerializeObject(world.Hook)
                                });
                            }

                            HookHash = newHash;

                            var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetWorldView, worldView.Value);
                            builder.Finish(q.Value);
                        }
                    }
                    await this.SendAsync(builder.DataBuffer, cancellationToken);

                    if (LeaderboardTime != (world.Leaderboard?.Time ?? 0))
                    {
                        LeaderboardTime = (world.Leaderboard?.Time ?? 0);

                        builder = new FlatBufferBuilder(1);

                        var stringName = builder.CreateString(world.Leaderboard?.ArenaRecord?.Name ?? " ");
                        var stringColor = builder.CreateString(world.Leaderboard?.ArenaRecord?.Color ?? " ");

                        NetLeaderboardEntry.StartNetLeaderboardEntry(builder);
                        NetLeaderboardEntry.AddColor(builder, stringColor);
                        NetLeaderboardEntry.AddName(builder, stringName);
                        NetLeaderboardEntry.AddScore(builder, world.Leaderboard?.ArenaRecord?.Score ?? 0);
                        NetLeaderboardEntry.AddToken(builder, !string.IsNullOrEmpty(world.Leaderboard?.ArenaRecord?.Token));
                        var record = NetLeaderboardEntry.EndNetLeaderboardEntry(builder);


                        var entriesVector = NetLeaderboard.CreateEntriesVector(builder, world.Leaderboard.Entries.Select(e =>
                        {
                            // the strings must be created into the buffer before the are referenced
                            // and before the start of the entry object
                            stringName = builder.CreateString(e.Name ?? string.Empty);
                            stringColor = builder.CreateString(e.Color ?? string.Empty);
                            StringOffset stringModeData = new StringOffset();

                            if (e.ModeData != null)
                                stringModeData = builder.CreateString(JsonConvert.SerializeObject(e.ModeData));

                            // here's the start of the entry object, after this we can only use
                            // predefined string offsets
                            NetLeaderboardEntry.StartNetLeaderboardEntry(builder);
                            NetLeaderboardEntry.AddFleetID(builder, e.FleetID);
                            NetLeaderboardEntry.AddName(builder, stringName);
                            NetLeaderboardEntry.AddColor(builder, stringColor);
                            NetLeaderboardEntry.AddScore(builder, e.Score);
                            NetLeaderboardEntry.AddPosition(builder, FromPositionVector(builder, e.Position));
                            NetLeaderboardEntry.AddToken(builder, !string.IsNullOrEmpty(e.Token));
                            if (e.ModeData != null)
                                NetLeaderboardEntry.AddModeData(builder, stringModeData);

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

                    while (Events.Count > 0)
                    {
                        var e = Events.Dequeue();

                        var eventType = builder.CreateString(e.EventType);
                        var eventData = builder.CreateString(e.Data);
                        NetEvent.StartNetEvent(builder);
                        NetEvent.AddType(builder, eventType);
                        NetEvent.AddData(builder, eventData);
                        var eventOffset = NetEvent.EndNetEvent(builder);

                        builder.Finish(NetQuantum.CreateNetQuantum(builder, AllMessages.NetEvent, eventOffset.Value).Value);
                        await this.SendAsync(builder.DataBuffer, cancellationToken);
                    }
                }
            }
            catch (WebSocketException)
            {
                //Console.WriteLine(e);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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

            await SendAsync(builder.DataBuffer, default);
        }

        private async Task HandlePingAsync(NetPing ping)
        {
            this.Backgrounded = ping.Backgrounded;
            this.ClientFPS = ping.Fps;
            this.ClientVPS = ping.Vps;
            this.ClientUPS = ping.Ups;
            this.ClientCS = ping.Cs;
            this.Bandwidth = ping.BandwidthThrottle;
            this.Latency = ping.Latency;

            if (player != null)
                player.Backgrounded = this.Backgrounded;

            await SendPingAsync();
        }

        private async Task HandleIncomingMessage(NetQuantum quantum)
        {
            switch (quantum.MessageType)
            {
                case AllMessages.NetPing:
                    var ping = quantum.Message<NetPing>().Value;
                    await HandlePingAsync(ping);
                    break;

                case AllMessages.NetSpawn:
                    var spawn = quantum.Message<NetSpawn>().Value;
                    var color = "red";

                    Sprites shipSprite = Sprites.ship_red;

                    player.Connection = this;
                    Logger.LogInformation($"Spawn: Name:\"{spawn.Name}\" Ship: {spawn.Ship} Score: {player.Score} Roles: {player.Roles}");


                    switch (spawn.Ship)
                    {
                        case "ship0":
                            shipSprite = Sprites.ship0;
                            color = "green";
                            break;
                        case "ship_secret":
                            if (player?.Roles?.Contains("Player") ?? false)
                            {
                                shipSprite = Sprites.ship_secret;
                                color = "yellow";
                            }
                            else
                            {
                                shipSprite = Sprites.ship_yellow;
                                color = "yellow";
                            }
                            break;
                        /*
                        shipSprite = Sprites.ship_secret;
                        color = "yellow";
                        break;
                        */
                        case "ship_zed":
                            if (player?.Roles?.Contains("Old Guard") ?? false)
                            {
                                shipSprite = Sprites.ship_zed;
                                color = "red";
                            }
                            else
                            {
                                shipSprite = Sprites.ship_red;
                                color = "red";
                            }
                            break;
                        /*
                        shipSprite = Sprites.ship_zed;
                        color = "red";
                        break;
                        */
                        case "ship_green":
                            shipSprite = Sprites.ship_green;
                            color = "green";
                            break;
                        case "ship_orange":
                            shipSprite = Sprites.ship_orange;
                            color = "orange";
                            break;
                        case "ship_pink":
                            shipSprite = Sprites.ship_pink;
                            color = "pink";
                            break;
                        case "ship_red":
                            shipSprite = Sprites.ship_red;
                            color = "red";
                            break;
                        case "ship_cyan":
                            shipSprite = Sprites.ship_cyan;
                            color = "cyan";
                            break;
                        case "ship_blue":
                            shipSprite = Sprites.ship_blue;
                            color = "blue";
                            break;
                        case "ship_yellow":
                            shipSprite = Sprites.ship_yellow;
                            color = "yellow";
                            break;
                    }

                    player.Spawn(spawn.Name, shipSprite, color, spawn.Token, spawn.Color);

                    break;
                case AllMessages.NetControlInput:
                    var input = quantum.Message<NetControlInput>().Value;

                    player?.SetControl(new ControlInput
                    {
                        Position = new Vector2(input.X, input.Y),
                        BoostRequested = input.Boost,
                        ShootRequested = input.Shoot,
                        CustomData = input.CustomData
                    });

                    if (input.SpectateControl == "action:next")
                    {
                        var next =
                            Player.GetWorldPlayers(world)
                                .Where(p => p.IsAlive)
                                .Where(p => p?.Fleet?.ID > (SpectatingFleet?.ID ?? 0))
                                .OrderBy(p => p?.Fleet?.ID)
                                .FirstOrDefault()?.Fleet;

                        if (next == null)
                            next = Player.GetWorldPlayers(world)
                                .Where(p => p.IsAlive)
                                .OrderBy(p => p?.Fleet?.ID)
                                .FirstOrDefault()?.Fleet;

                        SpectatingFleet = next;
                        IsSpectating = true;
                    }
                    else if (input.SpectateControl?.StartsWith("action:fleet:") ?? false)
                    {
                        var match = Regex.Match(input.SpectateControl, @"\d*$");
                        var fleetID = int.Parse(match.Value);

                        var next =
                            Player.GetWorldPlayers(world)
                                .Where(p => p.IsAlive)
                                .Where(p => p?.Fleet?.ID == fleetID)
                                .FirstOrDefault()?.Fleet;

                        SpectatingFleet = next;
                        IsSpectating = true;
                    }
                    else if (input.SpectateControl == "spectating")
                        IsSpectating = true;
                    else
                        IsSpectating = false;

                    break;

                case AllMessages.NetExit:
                    player.Exit();
                    break;

                case AllMessages.NetAuthenticate:
                    var auth = quantum.Message<NetAuthenticate>().Value;
                    if (player != null)
                    {
                        player.Token = auth.Token;
                        player.AuthenticationStarted = false;
                    }
                    break;
            }
        }

        public async Task ConnectAsync(HttpContext httpContext, WebSocket socket, CancellationToken cancellationToken = default(CancellationToken))
        {
            Socket = socket;

            var worldRequest = httpContext.Request.Query["world"].FirstOrDefault();

            this.Logger.LogInformation($"New Connection: {worldRequest}");

            world = Worlds.Find(worldRequest);

            var builder = new FlatBufferBuilder(1);
            await SendPingAsync();

            ConnectionHeartbeat.Register(this);

            try
            {
                lock (world.Bodies)
                {
                    player = new Player
                    {
                        IP = httpContext.Connection.RemoteIpAddress.ToString(),
                        Connection = this
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
                    player.Connection = null;
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
            catch (Exception e)
            {
                Console.WriteLine(e);
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