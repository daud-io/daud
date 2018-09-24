namespace Game.API.Client
{
    using Game.Engine.Networking.FlatBuffers;
    using Google.FlatBuffers;
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class PlayerConnection : IDisposable
    {
        private readonly APIClient APIClient;
        private readonly Timer PingTimer;
        private ClientWebSocket Socket = null;
        private const int PING_TIMER_MS = 1000;

        public PlayerConnection(APIClient apiClient)
        {
            APIClient = apiClient;
            PingTimer = new Timer(this.Ping, null, 1000, PING_TIMER_MS);
        }

        private void Ping(object state)
        {

        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Socket = await APIClient.ConnectWebSocket(APIEndpoint.PlayerConnect, cancellationToken: cancellationToken);
        }

        private Task HandleIncomingMessage(NetQuantum netQuantum)
        {
            switch (netQuantum.MessageType)
            {
                case AllMessages.NetPing:
                    var ping = netQuantum.Message<NetPing>().Value;
                    Console.WriteLine("Received NetPing");
                    break;
                case AllMessages.NetWorldView:
                    var view = netQuantum.Message<NetWorldView>().Value;
                    Console.WriteLine("Received NetWorldView");
                    break;
                default:
                    Console.WriteLine("Received other: " + netQuantum.MessageType.ToString());
                    break;
            }

            return Task.FromResult(0);
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
