namespace Game.Engine.Networking
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ConnectionHeartbeat
    {
        private volatile static List<Connection> Connections = new List<Connection>();
        private static readonly Timer heartbeat;
        private const int FREQUENCY = 20;

        static ConnectionHeartbeat()
        {
            heartbeat = new Timer((state) =>
            {
                Step();
            }, null, 0, FREQUENCY);
        }

        private static void Step()
        {
            lock (Connections)
            {
                Task.Run(async () =>
                {
                    foreach (var connection in Connections)
                        await connection.StepAsync();
                });
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
