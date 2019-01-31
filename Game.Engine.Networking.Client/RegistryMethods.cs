namespace Game.API.Client
{
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
    }
}
