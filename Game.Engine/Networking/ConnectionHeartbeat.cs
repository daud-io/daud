namespace Game.Engine.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ConnectionHeartbeat
    {
        private volatile static List<Connection> Connections = new List<Connection>();
        private static readonly Timer heartbeat;
        private const int FREQUENCY = 1000;

        static ConnectionHeartbeat()
        {
            /*heartbeat = new Timer((state) =>
            {
                Step();
            }, null, 0, FREQUENCY);*/
        }

        public static void Step()
        {
            lock (Connections)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(FREQUENCY);

                var cancellationToken = cts.Token;
                var start = DateTime.Now.Ticks;
                try
                {
                    Task.WhenAll(Connections.Select(c => c.StepAsync(cancellationToken)));
                }
                catch (Exception)
                {

                }

                long networkTicks = DateTime.Now.Ticks - start;

                if (networkTicks/10000 > 10)
                {
                    Console.WriteLine($"Network Ticks: {networkTicks}");
                }
            }
        }

        public static void Register(Connection connection)
        {
            lock (Connections)
                Connections.Add(connection);
        }

        public static void Unregister(Connection connection)
        {
            lock (Connections)
                Connections.Remove(connection);
        }
    }
}
