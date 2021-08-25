namespace Game.Engine.Networking
{
    using Newtonsoft.Json;
    using Game.API.Common;
    using Game.Engine.Core;
    using Game.Engine.Core.Steering;
    using global::FlatBuffers;
    using Game.Engine.Networking.FlatBuffers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Nito.AsyncEx;
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
        private Vector2 CameraPosition;
        private Vector2 CameraLinearVelocity;

        private const float VELOCITY_SCALE_FACTOR = 5000;

        private readonly ILogger<Connection> Logger = null;
        private readonly SemaphoreSlim WebsocketSendingSemaphore = new SemaphoreSlim(1, 1);
        private readonly BodyCache BodyCache = new BodyCache();

        private WebSocket Socket = null;

        private World World = null;
        private Player Player = null;

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
        public Fleet FollowFleet { get; private set; }

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

        private (Vector2, Vector2) DefineCamera()
        {
            Vector2 position = Vector2.Zero;
            Vector2 linearVelocity = Vector2.Zero;

            // First try to focus camera on the player if they have
            // a fleet alive;
            var followFleet = Player?.Fleet;

            // if the player doesn't have a fleet alive
            if (followFleet == null)
                // check to see if they are spectating a fleet that's alive
                if (SpectatingFleet != null && SpectatingFleet.Exists)
                    followFleet = SpectatingFleet;

            if (followFleet == null)
                // find someone else to watch
                followFleet = Player.GetWorldPlayers(World)
                    .ToList()
                    .Where(p => p.IsAlive)
                    .OrderByDescending(p => p.Score * 10000 + (10000 - p.Fleet?.ID ?? 0))
                    .FirstOrDefault()
                    ?.Fleet;

            // if we're watching a fleet, watch the center of their fleet
            if (followFleet != null)
            {
                this.FollowFleet = followFleet;

                var center = FleetMath.FleetCenterNaive(followFleet.Ships);
                position = center;
                linearVelocity = followFleet.FleetVelocity;
            }
            else
                this.FollowFleet = null;

            return (position, linearVelocity);
        }

        public void StepSyncInGameLoop()
        {
            var size = 6000;

            (this.CameraPosition, this.CameraLinearVelocity) = DefineCamera();
            
            lock (this.BodyCache) // wrong kind of lock but might do for now
            {
                BodyCache.Update(
                    World.BodiesNear(CameraPosition, size).ToList(),
                    World.Time
                );
            }
        }


        public async Task StepAsync(CancellationToken cancellationToken)
        {
            try
            {

                if (Player != null)
                {
                    var builder = new FlatBufferBuilder(1);

                    lock(BodyCache)
                    {
                        var updates = BodyCache.BodiesByError();
                        var updateBodies = updates.Take((int)this.Bandwidth);
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
                        foreach (var update in updateBodies)
                        {
                            var serverBody = update.Body;
                            if (update.Body.Exists)
                                update.UpdateSent(World.Time);

                            NetBody.CreateNetBody(builder,
                                Id: serverBody.ID,
                                DefinitionTime: update.ClientUpdatedTime,
                                originalPosition_X: (short)serverBody.Position.X,
                                originalPosition_Y: (short)serverBody.Position.Y,
                                velocity_X: (short)(serverBody.LinearVelocity.X * VELOCITY_SCALE_FACTOR),
                                velocity_Y: (short)(serverBody.LinearVelocity.Y * VELOCITY_SCALE_FACTOR),
                                OriginalAngle: (sbyte)(serverBody.Angle / MathF.PI * 127),
                                AngularVelocity: (sbyte)(serverBody.AngularVelocity * 10000),
                                Size: (byte)(serverBody.Size / 5),
                                Sprite: (ushort)serverBody.Sprite,
                                Mode: serverBody.Mode,
                                Group: serverBody.Group?.ID ?? 0);
                        }

                        var updatesVector = builder.EndVector();

                        var deletesVector = NetWorldView.CreateDeletesVector(builder, BodyCache.CollectStaleBuckets().Select(b =>
                            b.Body.ID
                        ).ToArray());

                        var messages = Player.GetMessages();
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
                        if (this.FollowFleet?.CustomData != null && this.FollowFleet.CustomData != CustomData)
                            customOffset = builder.CreateString(this.FollowFleet.CustomData);

                        NetWorldView.StartNetWorldView(builder);

                        // define camera
                        var cameraBody = NetBody.CreateNetBody(
                            builder,
                            Id: 0,
                            DefinitionTime: World.Time,
                            originalPosition_X: (short)(this.CameraPosition.X),
                            originalPosition_Y: (short)(this.CameraPosition.Y),
                            velocity_X: (short)(this.CameraLinearVelocity.X * VELOCITY_SCALE_FACTOR),
                            velocity_Y: (short)(this.CameraLinearVelocity.Y * VELOCITY_SCALE_FACTOR),
                            OriginalAngle: 0,
                            AngularVelocity: 0,
                            Size: 0,
                            Sprite: 0,
                            Mode: 0,
                            Group: 0
                        );

                        NetWorldView.AddCamera(builder, cameraBody);

                        NetWorldView.AddIsAlive(builder, Player?.IsAlive ?? false);
                        NetWorldView.AddTime(builder, World.Time);

                        NetWorldView.AddUpdates(builder, updatesVector);
                        NetWorldView.AddDeletes(builder, deletesVector);

                        NetWorldView.AddGroups(builder, groupsVector);
                        NetWorldView.AddGroupDeletes(builder, groupDeletesVector);
                        if (messages != null && messages.Any())
                            NetWorldView.AddAnnouncements(builder, announcementsVector);

                        if (this.FollowFleet?.CustomData != null && this.FollowFleet.CustomData != CustomData)
                            NetWorldView.AddCustomData(builder, customOffset);
                        CustomData = this.FollowFleet?.CustomData;

                        var players = Player.GetWorldPlayers(World);
                        NetWorldView.AddPlayerCount(builder, (uint)World.AdvertisedPlayerCount);
                        NetWorldView.AddSpectatorCount(builder, (uint)World.SpectatorCount);

                        NetWorldView.AddCooldownBoost(builder, (byte)((Player?.Fleet?.BoostCooldownStatus * 255) ?? 0));
                        NetWorldView.AddCooldownShoot(builder, (byte)((Player?.Fleet?.ShootCooldownStatus * 255) ?? 0));
                        NetWorldView.AddWorldSize(builder, (ushort)World.Hook.WorldSize);

                        if (this.FollowFleet != null)
                        {
                            // we've found someone to spectate, record it
                            if (this.FollowFleet != Player?.Fleet && this.FollowFleet != SpectatingFleet)
                                SpectatingFleet = this.FollowFleet;

                            // inform the client of which the fleet id
                            NetWorldView.AddFleetID(builder, (uint)this.FollowFleet.ID);
                        }
                        else
                            NetWorldView.AddFleetID(builder, 0);

                        var worldView = NetWorldView.EndNetWorldView(builder);

                        var newHash = World.Hook.GetHashCode();
                        if (HookHash != newHash)
                        {
                            this.Events.Enqueue(new BroadcastEvent
                            {
                                EventType = "hook",
                                Data = JsonConvert.SerializeObject(World.Hook)
                            });
                        }

                        HookHash = newHash;

                        var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetWorldView, worldView.Value);
                        builder.Finish(q.Value);
                    }

                    await this.SendAsync(builder.DataBuffer, cancellationToken);

                    if (LeaderboardTime != (World.Leaderboard?.Time ?? 0))
                    {
                        LeaderboardTime = (World.Leaderboard?.Time ?? 0);

                        builder = new FlatBufferBuilder(1);

                        var stringName = builder.CreateString(World.Leaderboard?.ArenaRecord?.Name ?? " ");
                        var stringColor = builder.CreateString(World.Leaderboard?.ArenaRecord?.Color ?? " ");

                        NetLeaderboardEntry.StartNetLeaderboardEntry(builder);
                        NetLeaderboardEntry.AddColor(builder, stringColor);
                        NetLeaderboardEntry.AddName(builder, stringName);
                        NetLeaderboardEntry.AddScore(builder, World.Leaderboard?.ArenaRecord?.Score ?? 0);
                        NetLeaderboardEntry.AddToken(builder, !string.IsNullOrEmpty(World.Leaderboard?.ArenaRecord?.Token));
                        var record = NetLeaderboardEntry.EndNetLeaderboardEntry(builder);

                        var entriesVector = NetLeaderboard.CreateEntriesVector(builder, World.Leaderboard.Entries.Select(e =>
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

                        var stringType = builder.CreateString(World.Leaderboard.Type ?? string.Empty);
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
            var pong = NetPing.CreateNetPing(builder, World.Time);
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

            if (Player != null)
                Player.Backgrounded = this.Backgrounded;

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

                    Player.Connection = this;
                    Logger.LogInformation($"Spawn: Name:\"{spawn.Name}\" Ship: {spawn.Ship} Score: {Player.Score} Roles: {Player.Roles}");


                    switch (spawn.Ship)
                    {
                        case "ship0":
                            shipSprite = Sprites.ship0;
                            color = "green";
                            break;
                        case "ship_secret":
                            if (Player?.Roles?.Contains("Player") ?? false)
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
                            if (Player?.Roles?.Contains("Old Guard") ?? false)
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

                    Player.Spawn(spawn.Name, shipSprite, color, spawn.Token, spawn.Color);

                    break;
                case AllMessages.NetControlInput:
                    var input = quantum.Message<NetControlInput>().Value;

                    Player?.SetControl(new ControlInput
                    {
                        Position = new Vector2(input.X, input.Y),
                        BoostRequested = input.Boost,
                        ShootRequested = input.Shoot,
                        CustomData = input.CustomData
                    });

                    if (input.SpectateControl == "action:next")
                    {
                        var next =
                            Player.GetWorldPlayers(World)
                                .Where(p => p.IsAlive)
                                .Where(p => p?.Fleet?.ID > (SpectatingFleet?.ID ?? 0))
                                .OrderBy(p => p?.Fleet?.ID)
                                .FirstOrDefault()?.Fleet;

                        if (next == null)
                            next = Player.GetWorldPlayers(World)
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
                            Player.GetWorldPlayers(World)
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
                    Player.Exit();
                    break;

                case AllMessages.NetAuthenticate:
                    var auth = quantum.Message<NetAuthenticate>().Value;
                    if (Player != null)
                    {
                        Player.Token = auth.Token;
                        Player.AuthenticationStarted = false;
                    }
                    break;
            }
        }

        public async Task ConnectAsync(HttpContext httpContext, WebSocket socket, CancellationToken cancellationToken = default(CancellationToken))
        {
            Socket = socket;

            var worldRequest = httpContext.Request.Query["world"].FirstOrDefault() ?? "default";

            this.Logger.LogInformation($"New Connection: {worldRequest}");

            World = Worlds.Find(worldRequest);

            var builder = new FlatBufferBuilder(1);
            await SendPingAsync();

            ConnectionHeartbeat.Register(this);

            try
            {
                lock (World.Bodies)
                {
                    Player = new Player
                    {
                        IP = httpContext.Connection.RemoteIpAddress.ToString(),
                        Connection = this
                    };
                    Player.Init(World);
                }

                var updateTask = StartSynchronizing(cancellationToken);
                var readTask = StartReadAsync(this.HandleIncomingMessage, cancellationToken);

                await Task.WhenAny(updateTask, readTask);

            }
            finally
            {
                ConnectionHeartbeat.Unregister(this);

                if (Player != null)
                {
                    Player.PendingDestruction = true;
                    Player.Connection = null;
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