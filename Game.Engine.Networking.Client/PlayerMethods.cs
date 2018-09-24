namespace Game.API.Client
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

        public async Task<PlayerConnection> ConnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var connection = new PlayerConnection(this.APIClient);
            await connection.ConnectAsync(cancellationToken);

            return connection;
        }
    }
}
