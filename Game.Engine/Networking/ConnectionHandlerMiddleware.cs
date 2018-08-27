namespace Game.Engine.Networking
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    public class ConnectionHandlerMiddleware
    {
        private readonly RequestDelegate Next;
        private readonly IServiceProvider ServiceProvider;

        public ConnectionHandlerMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            Next = next;
            ServiceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/api/v1/connect")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(context.Request.Headers));

                    using (var scope = ServiceProvider.CreateScope())
                    using (var connection = scope.ServiceProvider.GetService<Connection>())
                    using (var webSocket = await context.WebSockets.AcceptWebSocketAsync())
                        await connection.ConnectAsync(context, webSocket);
                }
                else
                {
                    Console.WriteLine("request to websocket endpoint was not a websocket request");

                    Console.WriteLine(JsonConvert.SerializeObject(context.Request.Headers));

                    context.Response.StatusCode = 426; // upgrade required
                }
            }
            else
                await Next(context);
        }
    }
}