namespace Game.Engine.Auditing
{
    using Firebase.Database;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public static class RemoteEventLog
    {
        private static Timer Heartbeat = null;
        private static readonly HttpClient HttpClient = new HttpClient();
        private static Queue<object> queue = new Queue<object>();
        private static readonly int POST_TIMER_MS = 1000;
        private static FirebaseClient Firebase;
        private static GameConfiguration GameConfiguration;
        private static bool Initialized = false;

        public static void Initialize(GameConfiguration gameConfiguration)
        {
            GameConfiguration = gameConfiguration;

            Heartbeat = new Timer((state) =>
            {
                PostData().Wait();
            }, null, 0, POST_TIMER_MS);


            if (gameConfiguration.FirebaseAuthKey != null)
                Firebase = new FirebaseClient(
                    gameConfiguration.FirebaseUrl,
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(gameConfiguration.FirebaseAuthKey)
                    });
            Initialized = true;
        }

        public static void SendEvent(object message)
        {
            if (Initialized)
                queue.Enqueue(message);
        }

        private async static Task PostData()
        {
            try
            {
                while (queue.Count > 0)
                {
                    var message = queue.Dequeue();
                    if (message is OnDeath)
                    {
                        if (GameConfiguration.DuelBotURL != null)
                            await HttpClient.PostAsJsonAsync(GameConfiguration.DuelBotURL, message);
                    }
                    else
                    {
                        if (Firebase != null)
                            await Firebase.Child("events").PostAsync(JsonConvert.SerializeObject(message));
                    }
                }
            }
            catch (Exception)
            { }
        }
    }
}
