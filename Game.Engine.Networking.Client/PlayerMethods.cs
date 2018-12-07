﻿namespace Game.API.Client
{
    using System.Threading;
    using System.Threading.Tasks;

    public class PlayerMethods
    {
        protected readonly APIClient APIClient;
        public PlayerMethods(APIClient apiClient)
        {
            this.APIClient = apiClient;
        }

        public async Task<Connection> ConnectAsync(
            string worldName = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var connection = new Connection(this.APIClient, worldName);
            await connection.ConnectAsync(cancellationToken);

            return connection;
        }
    }
}
