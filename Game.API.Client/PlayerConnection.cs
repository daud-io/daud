namespace Game.API.Client
{
    using Game.API.Common;
    using Game.API.Common.Models;
    using Game.Engine.Networking.FlatBuffers;
    using Google.FlatBuffers;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.WebSockets;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    public class PlayerConnection : IDisposable
    {
        public APIClient APIClient { get; private set; }
        public string WorldKey { get; private set; }
        public long GameTime { get; private set; }
        public ushort WorldSize { get; private set; }

        private readonly Timer PingTimer;
        private ClientWebSocket Socket = null;
        private const int PING_TIMER_MS = 1000;
        private readonly SemaphoreSlim WebsocketSendingSemaphore = new SemaphoreSlim(1, 1);

        public bool ControlIsBoosting { get; set; }
        public bool ControlIsShooting { get; set; }
        public float CooldownBoost { get; set; }
        public float CooldownShoot { get; set; }

        public string CustomData { get; set; }

        public Vector2 ControlAimTarget { get; set; }

        public bool IsAlive { get; private set; } = false;

        public int SimulateReceiveLatency = 0;

        private readonly BodyCache Cache = new BodyCache();

        public Vector2 Position { get; set; } = Vector2.Zero;

        public Func<Task> OnLeaderboard { get; set; } = null;
        public Func<Task> OnView { get; set; } = null;
        public Func<Task> OnConnected { get; set; } = null;

        public bool IsConnected { get; private set; } = false;

        public uint FleetID { get; set; } = 0;

        public Leaderboard Leaderboard { get; private set; } = null;

        public Hook Hook { get; set; }

        public Queue<Announcement> Announcements = new Queue<Announcement>();

        public PlayerConnection(string server, string worldName = null)
            : this(new APIClient(new Uri(server)), worldName)
        { }

        public PlayerConnection(APIClient apiClient, string worldName = null)
        {
            WorldKey = worldName;
            APIClient = apiClient;
            PingTimer = new Timer(this.PingEntry, null, 1000, PING_TIMER_MS);
        }

        private void PingEntry(object state)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (this.Socket != null && this.Socket.State == WebSocketState.Open)
                        await this.SendPingAsync();
                }
                catch (Exception)
                { }
            }).Wait();
        }

        private Task HandleNetPing(NetPing netPing)
        {
            return Task.FromResult(true);
        }

        private Task HandleNetEvent(NetEvent netEvent)
        {

            switch (netEvent.Type)
            {
                case "hook":
                    this.Hook = JsonConvert.DeserializeObject<Hook>(netEvent.Data);
                    break;
            }
            return Task.FromResult(true);
        }

        public IEnumerable<Body> Bodies
        {
            get
            {
                return Cache.Bodies;
            }
        }

        private async Task SendPingAsync()
        {
            var builder = new FlatBufferBuilder(1);
            var ping = NetPing.CreateNetPing(builder, bandwidthThrottle: 100);
            var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetPing, ping.Value);
            builder.Finish(q.Value);

            await SendAsync(builder.DataBuffer, default(CancellationToken));
        }

        public async Task SendControlInputAsync()
        {
            var builder = new FlatBufferBuilder(1);

            StringOffset customOffset = new StringOffset();

            if (CustomData != null)
            {
                customOffset = builder.CreateString(CustomData);
                Console.WriteLine("CustomData");
            }

            NetControlInput.StartNetControlInput(builder);

            NetControlInput.AddAngle(builder, 0);
            NetControlInput.AddBoost(builder, ControlIsBoosting);
            NetControlInput.AddX(builder, ControlAimTarget.X);
            NetControlInput.AddY(builder, ControlAimTarget.Y);
            NetControlInput.AddShoot(builder, ControlIsShooting);
            if (CustomData != null)
                NetControlInput.AddCustomData(builder, customOffset);
            var controlInput = NetControlInput.EndNetControlInput(builder);

            var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetControlInput, controlInput.Value);
            builder.Finish(q.Value);

            await SendAsync(builder.DataBuffer, default(CancellationToken));
            CustomData = null;
        }

        private Leaderboard.Entry EntryFromNetEntry(NetLeaderboardEntry? entry)
        {
            return entry != null
                ? new Leaderboard.Entry
                {
                    Color = entry?.Color,
                    FleetID = entry?.FleetID ?? 0,
                    ModeData = entry?.ModeData != null ? JsonConvert.DeserializeObject(entry?.ModeData) : null,
                    Name = entry?.Name,
                    Position = new Vector2(entry?.Position?.X ?? 0, entry?.Position?.Y ?? 0),
                    Score = entry?.Score ?? 0,
                    Token = (entry?.Token ?? false).ToString()
                }
                : null;
        }

        private async Task HandleNetLeaderboard(NetLeaderboard netLeaderboard)
        {
            var entryCount = netLeaderboard.EntriesLength;
            var leaderboard = new Leaderboard
            {
                ArenaRecord = EntryFromNetEntry(netLeaderboard.Record),
                Entries = new List<Leaderboard.Entry>()
            };

            for (var i = 0; i < entryCount; i++)
                leaderboard.Entries.Add(EntryFromNetEntry(netLeaderboard.Entries(i)));

            leaderboard.Type = netLeaderboard.Type;

            Leaderboard = leaderboard;

            if (OnLeaderboard != null)
                await OnLeaderboard();
        }

        private async Task HandleNetWorldView(NetWorldView netWorldView)
        {
            var updates = new List<Body>();

            for (int i = 0; i < netWorldView.UpdatesLength; i++)
            {
                var netBodyNullable = netWorldView.Updates(i);
                if (netBodyNullable.HasValue)
                {
                    var netBody = netBodyNullable.Value;

                    updates.Add(new Body
                    {
                        ID = netBody.Id,
                        DefinitionTime = netBody.DefinitionTime,
                        OriginalPosition = FromNetVector(netBody.OriginalPosition),
                        Momentum = FromNetVectorVelocity(netBody.Velocity),

                        OriginalAngle = netBody.OriginalAngle,
                        AngularVelocity = netBody.AngularVelocity,

                        Size = netBody.Size,
                        Sprite = (Sprites)netBody.Sprite,

                        GroupID = netBody.Group
                    });
                }
            }

            var deletes = new List<uint>();
            for (int i = 0; i < netWorldView.DeletesLength; i++)
                deletes.Add(netWorldView.Deletes(i));

            var groupDeletes = new List<uint>();
            for (int i = 0; i < netWorldView.GroupDeletesLength; i++)
                groupDeletes.Add(netWorldView.GroupDeletes(i));

            var groups = new List<Group>();
            for (int i = 0; i < netWorldView.GroupsLength; i++)
            {
                var netGroupNullable = netWorldView.Groups(i);
                if (netGroupNullable.HasValue)
                {
                    var group = netGroupNullable.Value;

                    groups.Add(new Group
                    {
                        ID = group.Group,
                        Caption = group.Caption,
                        Type = (GroupTypes)group.Type,
                        ZIndex = group.Zindex,
                        Owner = group.Owner,
                        Color = group.Color,
                        CustomData = group.CustomData
                    });
                }
            }

            Cache.Update(updates, deletes, groups, groupDeletes, netWorldView.Time);

            Position = FromNetVector(netWorldView.Camera.Value.OriginalPosition);

            IsAlive = netWorldView.IsAlive;
            FleetID = netWorldView.FleetID;
            GameTime = netWorldView.Time;
            WorldSize = netWorldView.WorldSize;
            CooldownBoost = netWorldView.CooldownBoost / 255f;
            CooldownShoot = netWorldView.CooldownShoot / 255f;

            if (netWorldView.AnnouncementsLength > 0)
            {
                for (var i = 0; i < netWorldView.AnnouncementsLength; i++)
                {
                    var announcement = netWorldView.Announcements(i);

                    Announcements.Enqueue(new Announcement
                    {
                        Type = announcement.Value.Type,
                        Text = announcement.Value.Text,
                        PointsDelta = announcement.Value.PointsDelta,
                        ExtraData = announcement.Value.ExtraData
                    });
                }
            }

            if (OnView != null)
                await OnView();
        }

        private Vector2 FromNetVectorVelocity(Vec2 vec2)
        {
            var VELOCITY_SCALE_FACTOR = 5000.0f;

            return new Vector2
            {
                X = vec2.X / VELOCITY_SCALE_FACTOR,
                Y = vec2.Y / VELOCITY_SCALE_FACTOR
            };

        }

        private Vector2 FromNetVector(Vec2 vec2)
        {
            return new Vector2
            {
                X = vec2.X,
                Y = vec2.Y
            };
        }

        public async Task SendExitAsync()
        {
            var builder = new FlatBufferBuilder(1);
            var exit = NetExit.CreateNetExit(builder);
            var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetExit, exit.Value);
            builder.Finish(q.Value);

            await SendAsync(builder.DataBuffer, default);
        }


        public async Task SpawnAsync(string name, string sprite, string color)
        {
            var builder = new FlatBufferBuilder(1);

            var stringName = builder.CreateString(name ?? string.Empty);
            var stringSprite = builder.CreateString(sprite ?? string.Empty);
            var stringColor = builder.CreateString(color ?? string.Empty);

            NetSpawn.StartNetSpawn(builder);
            NetSpawn.AddName(builder, stringName);
            NetSpawn.AddShip(builder, stringSprite);
            NetSpawn.AddColor(builder, stringColor);

            var spawn = NetSpawn.EndNetSpawn(builder);
            var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetSpawn, spawn.Value);
            builder.Finish(q.Value);

            await SendAsync(builder.DataBuffer, default(CancellationToken));
        }

        private async Task SendAsync(ByteBuffer byteBuffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = byteBuffer.ToSizedArray();
            await WebsocketSendingSemaphore.WaitAsync();
            try
            {
                var start = DateTime.Now;

                await Socket.SendAsync(
                    buffer,
                    WebSocketMessageType.Binary,
                    endOfMessage: true,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                WebsocketSendingSemaphore.Release();
            }
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Socket = await APIClient.ConnectWebSocket(
                APIEndpoint.PlayerConnect(WorldKey), cancellationToken: cancellationToken
            );

            IsConnected = true;

            if (OnConnected != null)
                await OnConnected();
        }

        private async Task HandleIncomingMessage(NetQuantum netQuantum)
        {
            switch (netQuantum.MessageType)
            {
                case AllMessages.NetEvent:
                    await HandleNetEvent(netQuantum.Message<NetEvent>().Value);
                    break;

                case AllMessages.NetPing:
                    await HandleNetPing(netQuantum.Message<NetPing>().Value);
                    break;

                case AllMessages.NetWorldView:
                    await HandleNetWorldView(netQuantum.Message<NetWorldView>().Value);
                    break;

                case AllMessages.NetLeaderboard:
                    await HandleNetLeaderboard(netQuantum.Message<NetLeaderboard>().Value);
                    break;

                default:
                    //Console.WriteLine("Received other: " + netQuantum.MessageType.ToString());
                    break;
            }
        }

        public async Task<bool> ListenAsync(CancellationToken cancellationToken = default)
        {
            await StartReadAsync(HandleIncomingMessage, cancellationToken);

            return true;
        }


        private async Task<bool> StartReadAsync(Func<NetQuantum, Task> onReceive, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = new WebSocketReceiveResult(0, WebSocketMessageType.Binary, false);

                while (!result.CloseStatus.HasValue && Socket.State == WebSocketState.Open)
                {
                    if (SimulateReceiveLatency > 0)
                        await Task.Delay(SimulateReceiveLatency);

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
                throw;
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
                        try { Socket.Dispose(); } catch (Exception) { }
                    if (PingTimer != null)
                        try { PingTimer.Dispose(); } catch (Exception) { }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PlayerConnection() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
