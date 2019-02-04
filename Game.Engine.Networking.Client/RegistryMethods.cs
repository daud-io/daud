namespace Game.API.Client
{
    using Game.API.Common.Models;
    using System.Net.Http;
    using System.Threading;
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

        public async Task<string> SuggestAsync(string configuredName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await RegistryClient.APICallAsync<string>(HttpMethod.Get, APIEndpoint.RegistrySuggestion, queryStringContent: new { configuredName }, cancellationToken: cancellationToken);
        }

        public async Task<bool> PostReportAsync(RegistryReport registryReport)
        {
            return await RegistryClient.APICallAsync<bool>(HttpMethod.Post, APIEndpoint.RegistryReport,
                bodyContent: registryReport);
        }
    }
}
