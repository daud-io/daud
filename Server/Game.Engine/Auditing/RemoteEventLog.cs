namespace Game.Engine.Auditing
{
/*    using Game.API.Client;
    using Game.API.Common.Models.Auditing;
    using Game.Engine.Core;
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
        private static GameConfiguration GameConfiguration;
        private static bool Initialized = false;
        private static RegistryClient RegistryClient;

        public static void Initialize(GameConfiguration gameConfiguration, RegistryClient registryClient)
        {
            GameConfiguration = gameConfiguration;
            RegistryClient = registryClient;

            Heartbeat = new Timer((state) =>
            {
                PostData().Wait();
            }, null, 0, POST_TIMER_MS);

            Initialized = true;
        }

        public static void SendEvent(object message)
        {
            if (Initialized)
                queue.Enqueue(message);
        }

        public static void SendEvent(AuditEventBase auditEvent, World world)
        {
            *auditEvent.WorldKey = world.WorldKey;
            auditEvent.AdvertisedPlayerCount = world.AdvertisedPlayerCount;
            auditEvent.GameID = world.GameID;
            auditEvent.GameTime = world.Time;
            auditEvent.Created = DateTime.Now;
            auditEvent.PublicURL = world.GameConfiguration.PublicURL;
            auditEvent.Type = auditEvent.GetType().Name;
            SendEvent(auditEvent);
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
                        if (RegistryClient != null)
                        {
                            await RegistryClient.Registry.PostEvents(new[] { message as AuditEventBase });
                        }
                    }
                }
            }
            catch (Exception)
            { }
        }
    }*/
}
