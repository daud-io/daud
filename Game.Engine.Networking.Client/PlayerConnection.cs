namespace Game.API.Client
{
    using Game.Engine.Networking.FlatBuffers;
    using Google.FlatBuffers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.WebSockets;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    public class Connection : IDisposable
    {
        private readonly APIClient APIClient;
        private readonly Timer PingTimer;
        private ClientWebSocket Socket = null;
        private const int PING_TIMER_MS = 1000;
        private readonly SemaphoreSlim WebsocketSendingSemaphore = new SemaphoreSlim(1, 1);

        public bool ControlIsBoosting { get; set; }
        public bool ControlIsShooting { get; set; }
        public Vector2 ControlAimTarget { get; set; }

        public bool IsAlive { get; private set; } = false;

        public int SimulateReceiveLatency = 0;

        private readonly BodyCache Cache = new BodyCache();

        public Func<Task> OnView { get; set; } = null;
        public Func<Task> OnConnected { get; set; } = null;

        public Connection(APIClient apiClient)
        {
            APIClient = apiClient;
            PingTimer = new Timer(this.PingEntry, null, 1000, PING_TIMER_MS);
        }

        private void PingEntry(object state)
        {
            Task.Run(async () =>
            {
                await this.SendPingAsync();
            }).Wait();
        }

        private async Task HandleNetPing(NetPing netPing)
        {
            //Console.WriteLine("Ping");
        }

        private async Task SendPingAsync()
        {
            var builder = new FlatBufferBuilder(1);
            var ping = NetPing.CreateNetPing(builder, 0);
            var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetPing, ping.Value);
            builder.Finish(q.Value);

            await SendAsync(builder.DataBuffer, default(CancellationToken));
        }

        public async Task SendControlInputAsync()
        {
            var builder = new FlatBufferBuilder(1);
            NetControlInput.StartNetControlInput(builder);

            NetControlInput.AddAngle(builder, 0);
            NetControlInput.AddBoost(builder, ControlIsBoosting);
            NetControlInput.AddX(builder, ControlAimTarget.X);
            NetControlInput.AddY(builder, ControlAimTarget.Y);
            NetControlInput.AddShoot(builder, ControlIsShooting);

            var controlInput = NetControlInput.EndNetControlInput(builder);

            var q = NetQuantum.CreateNetQuantum(builder, AllMessages.NetControlInput, controlInput.Value);
            builder.Finish(q.Value);

            await SendAsync(builder.DataBuffer, default(CancellationToken));
        }

        private async Task HandleNetWorldView(NetWorldView netWorldView)
        {
            var updates = new List<ProjectedBody>();

            for (int i = 0; i < netWorldView.UpdatesLength; i++)
            {
                var netBodyNullable = netWorldView.Updates(i);
                if (netBodyNullable.HasValue)
                {
                    var netBody = netBodyNullable.Value;

                    updates.Add(new ProjectedBody
                    {
                        ID = netBody.Id,
                        DefinitionTime = netBody.DefinitionTime,

                        OriginalAngle = netBody.OriginalAngle,
                        AngularVelocity = netBody.AngularVelocity,

                        OriginalPosition = FromNetVector(netBody.OriginalPosition),

                        Momentum = FromNetVector(netBody.Velocity) * 10f,
                        Size = netBody.Size
                    });
                }
            }
            var deletes = new List<uint>();
            for (int i = 0; i < netWorldView.DeletesLength; i++)
                deletes.Add(netWorldView.Deletes(i));

            Cache.Update(updates, deletes);

            IsAlive = netWorldView.IsAlive;

            if (OnView != null)
                await OnView();
        }

        private Vector2 FromNetVector(Vec2 vec2)
        {
            return new Vector2
            {
                X = vec2.X,
                Y = vec2.Y
            };
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
            Socket = await APIClient.ConnectWebSocket(APIEndpoint.PlayerConnect, cancellationToken: cancellationToken);

            if (OnConnected != null)
                await OnConnected();
        }

        private async Task HandleIncomingMessage(NetQuantum netQuantum)
        {
            switch (netQuantum.MessageType)
            {
                case AllMessages.NetPing:
                    await HandleNetPing(netQuantum.Message<NetPing>().Value);
                    break;

                case AllMessages.NetWorldView:
                    await HandleNetWorldView(netQuantum.Message<NetWorldView>().Value);
                    break;

                default:
                    Console.WriteLine("Received other: " + netQuantum.MessageType.ToString());
                    break;
            }
        }

        public async Task<bool> ListenAsync()
        {
            await StartReadAsync(HandleIncomingMessage);

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
