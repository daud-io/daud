namespace Game.Engine.Networking
{
    using Microsoft.AspNetCore.Builder;

    public static class ConnectionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGameWebsocketHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ConnectionHandlerMiddleware>();
        }
    }
}