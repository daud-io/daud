namespace Game.API.Client
{
    using Game.API.Common.Models;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class RegistryMethods
    {
        protected readonly RegistryClient RegistryClient;
        public RegistryMethods(RegistryClient registryClient)
        {
            this.RegistryClient = registryClient;
        }

        public async Task<bool> ListAsync()
        {
            return await RegistryClient.APICallAsync<bool>(HttpMethod.Get, APIEndpoint.Registry);
        }

        public async Task<bool> PostReportAsync(RegistryReport registryReport)
        {
            return await RegistryClient.APICallAsync<bool>(HttpMethod.Post, APIEndpoint.RegistryReport,
                bodyContent: registryReport);
        }
    }
}
