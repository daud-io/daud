namespace Game.API.Client
{
    using Game.API.Common;
    using Game.API.Common.Models;
    using DaudNet;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.WebSockets;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using FlatSharp;
    using FlatSharp.Attributes;

    public class PlayerConnection : IDisposable
    {
        public APIClient APIClient { get; private set; }
        public string WorldKey { get; private set; }
        public long GameTime { get; private set; }
        public uint Latency { get; set; }
        public DateTime PingSent { get; set; }
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
            this.Latency = (uint)DateTime.Now.Subtract(PingSent).TotalMilliseconds;
            return Task.FromResult(true);
        }

        private Task HandleNetEvent(NetEvent netEvent)
        {

            switch (netEvent.type)
            {
                case "hook":
                    this.Hook = JsonConvert.DeserializeObject<Hook>(netEvent.data);
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
            await SendAsync(new NetPing
            {
                bandwidththrottle = 100,
                latency = Latency
            });
        }

        public async Task SendControlInputAsync()
        {
            await SendAsync(new NetControlInput
            {
                angle = 0,
                boost = ControlIsBoosting,
                x = ControlAimTarget.X,
                y = ControlAimTarget.Y,
                shoot = ControlIsShooting,
                customdata = CustomData
            });
            
            CustomData = null;
        }

        private Leaderboard.Entry EntryFromNetEntry(NetLeaderboardEntry entry)
        {
            return new Leaderboard.Entry
            {
                Color = entry?.color,
                FleetID = entry?.fleetid ?? 0,
                ModeData = entry?.modedata != null ? JsonConvert.DeserializeObject(entry?.modedata) : null,
                Name = entry?.name,
                Position = new Vector2(entry?.position?.x ?? 0, entry?.position?.y ?? 0),
                Score = entry?.score ?? 0,
                Token = (entry?.token ?? false).ToString()
            };
        }

        private async Task HandleNetLeaderboard(NetLeaderboard netLeaderboard)
        {
            var entryCount = netLeaderboard.entries.Count;
            var leaderboard = new Leaderboard
            {
                ArenaRecord = EntryFromNetEntry(netLeaderboard.record),
                Entries = new List<Leaderboard.Entry>()
            };

            for (var i = 0; i < entryCount; i++)
                leaderboard.Entries.Add(EntryFromNetEntry(netLeaderboard.entries[i]));

            leaderboard.Type = netLeaderboard.type;

            Leaderboard = leaderboard;

            if (OnLeaderboard != null)
                await OnLeaderboard();
        }

        private async Task HandleNetWorldView(NetWorldView netWorldView)
        {
            var updates = new List<Body>();

            for (int i = 0; i < netWorldView.updates.Count; i++)
            {
                var netBody = netWorldView.updates[i];
                updates.Add(new Body
                {
                    ID = netBody.id,
                    DefinitionTime = netBody.definitiontime,
                    OriginalPosition = FromNetVector(netBody.originalposition),
                    Momentum = FromNetVectorVelocity(netBody.velocity),

                    OriginalAngle = netBody.originalangle,
                    AngularVelocity = netBody.angularvelocity,

                    Size = netBody.size,
                    Sprite = (Sprites)netBody.sprite,

                    GroupID = netBody.group
                });
            }

            var deletes = new List<uint>();
            for (int i = 0; i < netWorldView.deletes.Count; i++)
                deletes.Add(netWorldView.deletes[i]);

            var groupDeletes = new List<uint>();
            for (int i = 0; i < netWorldView.groupdeletes.Count; i++)
                groupDeletes.Add(netWorldView.groupdeletes[i]);

            var groups = new List<Group>();
            for (int i = 0; i < netWorldView.groups.Count; i++)
            {
                var group = netWorldView.groups[i];
                groups.Add(new Group
                {
                    ID = group.group,
                    Caption = group.caption,
                    Type = (GroupTypes)group.type,
                    ZIndex = group.zindex,
                    Owner = group.owner,
                    Color = group.color,
                    CustomData = group.customdata
                });
            }

            Cache.Update(updates, deletes, groups, groupDeletes, netWorldView.time);

            Position = FromNetVector(netWorldView.camera.originalposition);

            IsAlive = netWorldView.isalive;
            FleetID = netWorldView.fleetid;
            GameTime = netWorldView.time;
            WorldSize = netWorldView.worldsize;
            CooldownBoost = netWorldView.cooldownboost / 255f;
            CooldownShoot = netWorldView.cooldownshoot / 255f;

            foreach (var announcement in netWorldView.announcements)
                Announcements.Enqueue(new Announcement
                {
                    Type = announcement.type,
                    Text = announcement.text,
                    PointsDelta = announcement.pointsdelta,
                    ExtraData = announcement.extradata
                });


            if (OnView != null)
                await OnView();
        }

        private Vector2 FromNetVectorVelocity(Vec2 vec2)
        {
            var VELOCITY_SCALE_FACTOR = 5000.0f;

            return new Vector2
            {
                X = vec2.x / VELOCITY_SCALE_FACTOR,
                Y = vec2.y / VELOCITY_SCALE_FACTOR
            };

        }

        private Vector2 FromNetVector(Vec2 vec2)
        {
            return new Vector2
            {
                X = vec2.x,
                Y = vec2.y
            };
        }

        public async Task SendExitAsync()
        {
            await SendAsync(new NetExit());
        }

        public void CacheClear()
        {
            Cache.Clear();
        }

        public async Task SpawnAsync(string name, string sprite, string color)
        {
            //CacheClear();
            await SendAsync(new NetSpawn
            {
                name = name,
                ship = sprite,
                color = color
            });
        }

        private async Task SendAsync(object message, CancellationToken cancellationToken = default(CancellationToken))
        {
            
            int maxBytesNeeded = FlatBufferSerializer.Default.GetMaxSize(message);
            byte[] buffer = new byte[maxBytesNeeded];
            int bytesWritten = FlatBufferSerializer.Default.Serialize(message, buffer);

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

            switch (netQuantum.message.Kind)
            {
                case AllMessages.ItemKind.NetEvent:
                    await HandleNetEvent(netQuantum.message.NetEvent);
                    break;

                case AllMessages.ItemKind.NetPing:
                    await HandleNetPing(netQuantum.message.NetPing);
                    break;

                case AllMessages.ItemKind.NetWorldView:
                    await HandleNetWorldView(netQuantum.message.NetWorldView);
                    break;

                case AllMessages.ItemKind.NetLeaderboard:
                    await HandleNetLeaderboard(netQuantum.message.NetLeaderboard);
                    break;

                default:
                    Console.WriteLine("Received other: " + netQuantum.message.Kind.ToString());
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
                                var quantum = FlatBufferSerializer.Default.Parse<NetQuantum>(bytes);

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
