namespace Game.Engine.Networking
{
    using Game.Engine.Core;
    using Game.Engine.Core.Actors;
    using Game.Models.Messages;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class Connection : IDisposable
    {
        private readonly ILogger<Connection> Logger = null;
        private readonly SemaphoreSlim WebsocketSendingSemaphore = new SemaphoreSlim(1, 1);
        private static List<Connection> connections = new List<Connection>();

        private WebSocket Socket = null;

        private World world = null;
        private Player player = null;

        public Connection(ILogger<Connection> logger)
        {
            this.Logger = logger;
        }

        public async Task StepAsync(CancellationToken cancellationToken)
        {
            if (player != null)
            {
                var view = new View
                {
                    PlayerView = player.View
                };
                await this.SendAsync(view, cancellationToken);
            }
        }

        private async Task SendAsync(MessageBase message, CancellationToken cancellationToken)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);

            await SendAsync(buffer, cancellationToken);
        }

        public async Task SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            await WebsocketSendingSemaphore.WaitAsync();
            try
            {
                var start = DateTime.Now;

                await Socket.SendAsync(
                    buffer, 
                    WebSocketMessageType.Text, 
                    endOfMessage: true, 
                    cancellationToken: cancellationToken);

                //Console.WriteLine($"{DateTime.Now.Subtract(start).TotalMilliseconds}ms in send");
            }
            finally
            {
                WebsocketSendingSemaphore.Release();
            }
        }

        private async Task HandleIncomingMessage(MessageBase message)
        {
            if (message is Ping)
            {
                await SendAsync(message, default(CancellationToken));
            }
            else if (message is Spawn)
            {
                var s = message as Spawn;

                if (player == null)
                {
                    player = new Player()
                    {
                        Name = s.Name
                    };
                    player.Init(world);
                }

                player.Spawn();
            }
            else if (message is ControlInput)
            {
                var s = message as ControlInput;

                if (player != null)
                {
                    player.Angle = s.Angle;
                    player.BoostRequested = s.BoostRequested;
                    player.ShootRequested = s.ShootRequested;
                    player.Name = s.Name;
                    player.Ship = s.Ship;
                }
            }
        }

        public async Task ConnectAsync(HttpContext httpContext, WebSocket socket, CancellationToken cancellationToken = default(CancellationToken))
        {
            Socket = socket;

            world = Worlds.Find();

            await this.SendAsync(new Hello(), cancellationToken);

            ConnectionHeartbeat.Register(this);

            try
            {
                await StartReadAsync(this.HandleIncomingMessage, cancellationToken);
            }
            finally
            {
                ConnectionHeartbeat.Unregister(this);

                if (player != null)
                    player.Deinit();
            }
        }

        private async Task<bool> StartReadAsync(Func<MessageBase, Task> onReceive, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = new WebSocketReceiveResult(0, WebSocketMessageType.Text, false);

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
                                var json = Encoding.UTF8.GetString(bytes, 0, (int)ms.Length);
                                var item = JObject.Parse(json);

                                var enumType = (MessageBase.MessageTypes)item["Type"].Value<int>();
                                var type = MessageBase.MessageTypeMap[enumType];

                                MessageBase message = item.ToObject(type) as MessageBase;

                                await onReceive(message);

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