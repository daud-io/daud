namespace Game.Engine.Networking.Client
{
    using Game.Engine.Networking.FlatBuffers;
    using Google.FlatBuffers;
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class GameConnection
    {
        private WebSocket Socket = null;

        public async Task<ClientWebSocket> Connect(string server, CancellationToken cancellationToken = default(CancellationToken))
        {
            var webSocket = new ClientWebSocket();
            //webSocket.Options.SetRequestHeader("Authorization", "Bearer " + Token);

            await webSocket.ConnectAsync(new Uri(server), cancellationToken);

            var readTask = StartReadAsync(this.HandleIncomingMessage, cancellationToken);

            return webSocket;
        }

        private Task HandleIncomingMessage(NetQuantum netQuantum)
        {
            switch (netQuantum.MessageType)
            {
                case AllMessages.NetPing:
                    var ping = netQuantum.Message<NetPing>().Value;
                    Console.WriteLine("Received NetPing");
                    break;
            }

            return Task.FromResult(0);
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



    }
}
