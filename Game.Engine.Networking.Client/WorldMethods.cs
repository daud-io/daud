namespace Game.API.Client
{
    using Game.API.Common.Models;
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

        public async Task<bool> SetMapTiles(string worldKey, IEnumerable<MapTileModel> tiles, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await APIClient.APICallAsync<bool>(
                HttpMethod.Post, 
                APIEndpoint.WorldMap, 
                queryStringContent: new
                {
                    worldKey
                },
                bodyContent: tiles,
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

        public async Task<string> HookAsync(object hook, string worldName = null)
        {
            return await APIClient.APICallAsync<string>(
                HttpMethod.Post, APIEndpoint.ServerHook, queryStringContent: new { worldName }, bodyContent: hook);
        }
    }
}
