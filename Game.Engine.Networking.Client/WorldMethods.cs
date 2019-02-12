namespace Game.API.Client
{
    using Game.API.Common.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class WorldMethods
    {
        protected readonly APIClient APIClient;
        public WorldMethods(APIClient apiClient)
        {
            this.APIClient = apiClient;
        }

        public async Task<string> PostHookAsync(object hook, string worldName = null)
        {
            var response = await APIClient.APICallAsync<object>(
                HttpMethod.Post, APIEndpoint.WorldHook, queryStringContent: new { worldName }, bodyContent: hook);

            return JsonConvert.SerializeObject(response, Formatting.Indented);
        }

        public async Task<string> PutWorldAsync(string worldKey, object hook)
        {
            return await APIClient.APICallAsync<string>(
                HttpMethod.Put, APIEndpoint.World,
                queryStringContent: new
                {
                    worldKey,
                    hookJson = JsonConvert.SerializeObject(hook)
                });
        }

        public async Task<string> DeleteWorldAsync(string worldKey)
        {
            return await APIClient.APICallAsync<string>(
                HttpMethod.Delete, APIEndpoint.World,
                queryStringContent: new
                {
                    worldKey
                });
        }

        public async Task<bool> SetMap(string worldKey, MapModel mapModel, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await APIClient.APICallAsync<bool>(
                HttpMethod.Post, 
                APIEndpoint.WorldMap, 
                queryStringContent: new
                {
                    worldKey
                },
                bodyContent: mapModel,
                cancellationToken: cancellationToken
            );
        }

        public async Task<bool> ServerResetAsync()
        {
            try
            {
                await APIClient.APICallAsync<bool>(HttpMethod.Post, APIEndpoint.ServerReset);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AnnounceAsync(string message, string worldName = null)
        {
            return await APIClient.APICallAsync<bool>(HttpMethod.Post, APIEndpoint.ServerAnnounce, 
                queryStringContent: new { message, worldName});
        }

        public async Task<IEnumerable<GameConnection>> ConnectionsAsync(string worldName = null)
        {
            return await APIClient.APICallAsync<IEnumerable<GameConnection>>(
                HttpMethod.Get, APIEndpoint.ServerPlayers,
                queryStringContent: new { worldName }
            );
        }
    }
}
