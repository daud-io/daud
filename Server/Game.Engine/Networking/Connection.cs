namespace Game.Engine.Networking
{
    using Newtonsoft.Json;
    using Game.API.Common;
    using Game.Engine.Core;
    using Game.Engine.Core.Steering;
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
    using DaudNet;
    using FlatSharp;

    public class Connection : IDisposable
    {
        private Vector2 CameraPosition;
        private Vector2 CameraLinearVelocity;

        private const float VELOCITY_SCALE_FACTOR = 5000;

        private readonly ILogger<Connection> Logger = null;
        private readonly SemaphoreSlim WebsocketSendingSemaphore = new SemaphoreSlim(1, 1);
        private readonly BodyCache BodyCache = new BodyCache();

        public readonly AsyncAutoResetEvent WorldUpdateEvent;
        private WebSocket Socket = null;

        private World World = null;
        private Player Player = null;

        private int HookHash = 0;
        private long LeaderboardTime = 0;


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
            this.WorldUpdateEvent = new AsyncAutoResetEvent();
        }

        public async Task StartSynchronizing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await WorldUpdateEvent.WaitAsync(cancellationToken);
                await StepAsync(cancellationToken);
            }
        }

        private Vec2 FromPositionVector(Vector2 vector)
        {
            return new Vec2
            {
                x = (short)vector.X, 
                y = (short)vector.Y
            };
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

            return (position, linearVelocity);
        }

        public void StepSyncInGameLoop()
        {
            var size = 6000;

            (this.CameraPosition, this.CameraLinearVelocity) = DefineCamera();
            
            lock (this.BodyCache)
            {
                BodyCache.Update(
                    World.BodiesNear(CameraPosition, size).ToList(),
                    World.Time
                );
            }
            WorldUpdateEvent.Set();
        }

        public async Task StepAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (Player != null)
                {
                    var netWorldView = new NetWorldView();

                    lock(BodyCache)
                    {

                        if (this.Bandwidth > 0
                            && !this.Backgrounded)
                        {
                        
                            var updates = BodyCache.BodiesByError();
                            var updateBodies = updates.Take((int)this.Bandwidth * 2);
                            var updatedGroups = BodyCache.GroupsByError().ToList();

                            netWorldView.groups = updatedGroups.Select(b =>
                            {
                                var serverGroup = b.GroupUpdated;

                                var group = new NetGroup
                                {
                                    group = serverGroup.ID,
                                    type = (byte)serverGroup.GroupType,
                                    caption = serverGroup.Caption,
                                    zindex = serverGroup.ZIndex,
                                    owner = serverGroup.OwnerID,
                                    color = serverGroup.Color,
                                    customdata = serverGroup.CustomData
                                };

                                return group;
                            }).ToList();


                            foreach (var update in updatedGroups)
                                update.GroupClient = update.GroupUpdated.Clone();

                            netWorldView.groupdeletes = BodyCache.CollectStaleGroups().Select(b =>
                                b.GroupUpdated.ID
                            ).ToList();

                            netWorldView.updates = new List<NetBody>();
                            foreach (var update in updateBodies)
                            {
                                update.UpdateSent(World.Time);
                                
                                netWorldView.updates.Add(new NetBody
                                {
                                    id = update.ID,
                                    definitiontime = update.ClientUpdatedTime,
                                    originalposition = new Vec2
                                    {
                                        x = (short)update.Position.X,
                                        y = (short)update.Position.Y
                                    },
                                    velocity = new Vec2
                                    {
                                        x = (short)(update.LinearVelocity.X * VELOCITY_SCALE_FACTOR),
                                        y = (short)(update.LinearVelocity.Y * VELOCITY_SCALE_FACTOR)
                                    },
                                    originalangle = (sbyte)(update.Angle / MathF.PI * 127),
                                    angularvelocity = (sbyte)(update.AngularVelocity * 10000),
                                    size = (byte)(update.Size / 5),
                                    sprite = (ushort)update.Sprite,
                                    mode = update.Mode,
                                    group = update.GroupID
                                });
                            }

                            netWorldView.deletes = BodyCache.CollectStaleBuckets().Select(b =>
                                b.ID
                            ).ToList();
                        }

                        var messages = Player.GetMessages();
                        if (messages != null && messages.Any())
                        {
                            netWorldView.announcements = messages.Select(e =>
                            {
                                return new NetAnnouncement
                                {
                                    type = e.Type,
                                    text = e.Message,
                                    extradata = e.ExtraData != null
                                        ? JsonConvert.SerializeObject(e.ExtraData)
                                        : null,
                                    pointsdelta = e.PointsDelta
                                };

                            }).ToList();
                        }

                        // define camera
                        netWorldView.camera = new NetBody
                        {
                            definitiontime = World.Time,
                            originalposition = new Vec2
                            {
                                x = (short)(this.CameraPosition.X),
                                y = (short)(this.CameraPosition.Y)
                            },
                            velocity = new Vec2
                            {
                                x = (short)(this.CameraLinearVelocity.X * VELOCITY_SCALE_FACTOR),
                                y = (short)(this.CameraLinearVelocity.Y * VELOCITY_SCALE_FACTOR)
                            }
                        };

                        netWorldView.isalive = Player?.IsAlive ?? false;
                        netWorldView.time = World.Time;

                        netWorldView.customdata = this.FollowFleet?.CustomData;

                        netWorldView.playercount = (uint)World.AdvertisedPlayerCount;
                        netWorldView.spectatorcount = (uint)World.SpectatorCount;
                        netWorldView.cooldownboost = (byte)((Player?.Fleet?.BoostCooldownStatus * 255) ?? 0);
                        netWorldView.cooldownshoot = (byte)((Player?.Fleet?.ShootCooldownStatus * 255) ?? 0);
                        netWorldView.worldsize = (ushort)World.Hook.WorldSize;

                        if (this.FollowFleet != null)
                        {
                            // we've found someone to spectate, record it
                            if (this.FollowFleet != Player?.Fleet && this.FollowFleet != SpectatingFleet)
                                SpectatingFleet = this.FollowFleet;

                            // inform the client of which the fleet id
                            netWorldView.fleetid = (uint)this.FollowFleet.ID;
                        }
                        else
                            netWorldView.fleetid = 0;


                        if (HookHash != World.HookHash)
                        {

                            this.Events.Enqueue(new BroadcastEvent
                            {
                                EventType = "hook",
                                Data = JsonConvert.SerializeObject(World.Hook)
                            });
                            HookHash = World.HookHash;
                        }
                    }

                    await this.SendAsync(new AllMessages(netWorldView), cancellationToken);

                    if (LeaderboardTime != (World.Leaderboard?.Time ?? 0))
                    {
                        LeaderboardTime = (World.Leaderboard?.Time ?? 0);
                        await this.SendAsync(new AllMessages(new NetLeaderboard
                        {
                            record = new NetLeaderboardEntry
                            {
                                name = World.Leaderboard?.ArenaRecord?.Name ?? " ",
                                color = World.Leaderboard?.ArenaRecord?.Color ?? " ",
                                score = World.Leaderboard?.ArenaRecord?.Score ?? 0,
                                token = !string.IsNullOrEmpty(World.Leaderboard?.ArenaRecord?.Token)
                            },
                            entries = World.Leaderboard.Entries.Select(
                                e => new NetLeaderboardEntry
                                {
                                    fleetid = e.FleetID,
                                    name = e.Name,
                                    color = e.Color,
                                    score = e.Score,
                                    position = FromPositionVector(e.Position),
                                    token = !string.IsNullOrEmpty(e.Token),
                                    modedata = (e.ModeData != null)
                                        ? JsonConvert.SerializeObject(e.ModeData)
                                        : null
                                }
                            ).ToList(),
                            type = World.Leaderboard.Type
                        }), cancellationToken);
                    }

                    while (Events.Count > 0)
                    {
                        var e = Events.Dequeue();

                        await this.SendAsync(new AllMessages(new NetEvent
                        {
                            type = e.EventType,
                            data = e.Data
                        }), cancellationToken);
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

        private async Task SendAsync(AllMessages message, CancellationToken cancellationToken = default)
        {
            var q = new NetQuantum
            {
                message = message
            };

            int maxBytesNeeded = FlatBufferSerializer.Default.GetMaxSize(q);
            byte[] buffer = new byte[maxBytesNeeded];
            int bytesWritten = FlatBufferSerializer.Default.Serialize(q, buffer);

            await WebsocketSendingSemaphore.WaitAsync();
            try
            {
                if (Socket.State == WebSocketState.Open)
                {
                    var start = DateTime.Now;

                    await Socket.SendAsync(
                        buffer,
                        WebSocketMessageType.Binary,
                        endOfMessage: true,
                        cancellationToken: cancellationToken);

                    //Console.WriteLine($"{DateTime.Now.Subtract(start).TotalMilliseconds}ms in send");
                }
                else
                {
                    throw new Exception("Connection is closed, cannot write");
                }
            }
            catch(WebSocketException)
            {
                // client disconnected
            }
            finally
            {
                WebsocketSendingSemaphore.Release();
            }
        }
 
        private async Task HandlePingAsync(NetPing ping)
        {
            this.Backgrounded = ping.backgrounded;
            this.ClientFPS = ping.fps;
            this.ClientVPS = ping.vps;
            this.ClientUPS = ping.ups;
            this.ClientCS = ping.cs;
            this.Bandwidth = ping.bandwidththrottle;
            this.Latency = ping.latency;

            if (Player != null)
                Player.Backgrounded = this.Backgrounded;

            ping.time = World.Time;
            ping.clienttime = ping.clienttime;

            await SendAsync(new AllMessages(ping));
        }

        private async Task HandleIncomingMessage(NetQuantum quantum)
        {
            switch (quantum.message.Kind)
            {
                case AllMessages.ItemKind.NetPing:
                    await HandlePingAsync(quantum.message.NetPing);
                    break;

                case AllMessages.ItemKind.NetSpawn:
                    var spawn = quantum.message.NetSpawn;
                    var color = "red";

                    Sprites shipSprite = Sprites.ship_red;

                    Player.Connection = this;
                    Logger.LogInformation($"Spawn: Name:\"{spawn.name}\" Ship: {spawn.ship} Score: {Player.Score} Roles: {Player.Roles}");


                    switch (spawn.ship)
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

                    Player.Spawn(spawn.name, shipSprite, color, spawn.token, spawn.color);

                    break;
                case AllMessages.ItemKind.NetControlInput:
                    var input = quantum.message.NetControlInput;

                    Player?.SetControl(new ControlInput
                    {
                        Position = new Vector2(input.x, input.y),
                        BoostRequested = input.boost,
                        ShootRequested = input.shoot,
                        CustomData = input.customdata
                    });

                    if (input.spectatecontrol == "action:next")
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
                    else if (input.spectatecontrol?.StartsWith("action:fleet:") ?? false)
                    {
                        var match = Regex.Match(input.spectatecontrol, @"\d*$");
                        var fleetID = int.Parse(match.Value);

                        var next =
                            Player.GetWorldPlayers(World)
                                .Where(p => p.IsAlive)
                                .Where(p => p?.Fleet?.ID == fleetID)
                                .FirstOrDefault()?.Fleet;

                        SpectatingFleet = next;
                        IsSpectating = true;
                    }
                    else if (input.spectatecontrol == "spectating")
                        IsSpectating = true;
                    else
                        IsSpectating = false;

                    break;

                case AllMessages.ItemKind.NetExit:
                    Player.Exit();
                    break;

                case AllMessages.ItemKind.NetAuthenticate:
                    var auth = quantum.message.NetAuthenticate;
                    if (Player != null)
                    {
                        Player.Token = auth.token;
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

            //await SendPingAsync()

            lock(World.Connections)
                World.Connections.Add(this);

            try
            {
                
                Player = World.CreatePlayer();
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                Player.IP = forwardedFor ?? httpContext.Connection.RemoteIpAddress.ToString();
                Player.Connection = this;

                var updateTask = StartSynchronizing(cancellationToken);
                var readTask = StartReadAsync(this.HandleIncomingMessage, cancellationToken);

                await Task.WhenAny(updateTask, readTask);

            }
            finally
            {
                lock(World.Connections)
                    World.Connections.Remove(this);

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
                                var quantum = FlatBufferSerializer.Default.Parse<NetQuantum>(bytes);

                                await onReceive(quantum);

                                result = new WebSocketReceiveResult(0, WebSocketMessageType.Text, false);
                            }
                        }
                    }
                }

                return true;
            }
            catch (WebSocketException )
            {
                // client disconnected
                return false;
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