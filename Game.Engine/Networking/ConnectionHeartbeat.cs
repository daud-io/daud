namespace Game.Engine.Networking
{
    using System.Collections.Generic;

    public static class ConnectionHeartbeat
    {
        private volatile static List<Connection> Connections = new List<Connection>();

        static ConnectionHeartbeat()
        {
        }

        public static void Step()
        {
            lock (Connections)
            {
                foreach (var connection in Connections)
                    connection.WorldUpdateEvent.Set();
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
