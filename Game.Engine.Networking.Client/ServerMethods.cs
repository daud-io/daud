namespace Game.API.Client
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class ServerMethods
    {
        protected readonly APIClient APIClient;
        public ServerMethods(APIClient apiClient)
        {
            this.APIClient = apiClient;
        }

        public async Task<bool> HealthGetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var started = DateTime.Now;
                await APIClient.APICallAsync<bool>(HttpMethod.Get, APIEndpoint.HealthGet, cancellationToken: cancellationToken);
                var totaltime = DateTime.Now.Subtract(started).TotalMilliseconds;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
