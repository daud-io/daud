namespace Game.Engine.Networking
{
    using Game.API.Client;
    using Game.API.Common.Models;
    using Game.Engine.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class RegistryHandling : IDisposable
    {
        private Timer Heartbeat = null;

        private readonly List<IDisposable> Disposables = new List<IDisposable>();
        private bool Processing = false;
        public int ReportingTimerDelay { get; set; } = 2000;
        public int HTTPPostTimeout { get; set; } = 2000;
        private RegistryClient RegistryClient;
        private readonly GameConfiguration GameConfiguration;

        public RegistryHandling(RegistryClient registryClient, GameConfiguration gameConfiguration)
        {
            this.RegistryClient = registryClient;
            this.GameConfiguration = gameConfiguration;

            if (this.GameConfiguration.RegistryEnabled)
                InitializeStepTimer();
        }

        private void Step()
        {
            lock (this)
            {
                Processing = true;

                var herokuAppName = Environment.GetEnvironmentVariable("HEROKU_APP_NAME");

                if (herokuAppName != null)
                    herokuAppName = $"{herokuAppName}.herokuapp.com";

                var report = new RegistryReport
                {
                    URL = GameConfiguration.PublicURL ?? herokuAppName,
                    Worlds = Worlds.AllWorlds.Select(w => new RegistryReport.World
                    {
                        AdvertisedPlayers = w.Value.AdvertisedPlayerCount,
                        Hook = w.Value.Hook,
                        WorldKey = w.Key
                    }).ToList()
                };

                try
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            return RegistryClient.Registry.PostReportAsync(report);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Registration failure: {e.Message}");
                        }

                        return Task.FromResult(0);
                    }).Wait(HTTPPostTimeout);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Registration failure: {e.Message}");
                }
                Processing = false;
            }
        }


        private void InitializeStepTimer()
        {
            Heartbeat = new Timer((state) =>
            {
                if (Processing)
                    return;

                Step();
                ConnectionHeartbeat.Step();

            }, null, 0, ReportingTimerDelay);
            Disposables.Add(Heartbeat);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
                if (disposing)
                {
                    foreach (var d in Disposables)
                        try
                        {
                            d.Dispose();
                        }
                        catch (Exception) { }
                }
            disposedValue = true;
        }
        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
