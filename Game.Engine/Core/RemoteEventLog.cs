namespace Game.Engine.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public static class RemoteEventLog
    {
        private static readonly Timer Heartbeat = null;
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly Queue<object> queue = new Queue<object>();
        private static readonly int POST_TIMER_MS = 1000;

        static RemoteEventLog()
        {
            Heartbeat = new Timer((state) =>
            {
                PostData().Wait();
            }, null, 0, POST_TIMER_MS);
        }

        public static void SendEvent(object message)
        {
            queue.Enqueue(message);
        }

        private async static Task PostData()
        {
            try
            {
                while (queue.Count > 0)
                {
                    var message = queue.Dequeue();

                    var response = await HttpClient.PostAsJsonAsync("https://daud-discord.glitch.me/", message);

                    if (response.IsSuccessStatusCode)
                    {
                        // cool
                    }
                    else
                    {
                        //
                    }
                }
            }
            catch (Exception)
            { }
        }
    }
}
