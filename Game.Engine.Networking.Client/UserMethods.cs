namespace Game.API.Client
{
    using Game.API.Common.Models;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class UserMethods
    {
        protected readonly APIClient APIClient;
        public UserMethods(APIClient apiClient)
        {
            this.APIClient = apiClient;
        }

        public Task<TokenResponseModel> AuthenticateAsync(
            UserIdentifier userIdentifier,
            string password,
            CancellationToken cancellationToken = default(CancellationToken)
        ) => AuthenticateAsync(new TokenRequestModel { Identifier = userIdentifier, Password = password });

        public async Task<TokenResponseModel> AuthenticateAsync(
            TokenRequestModel tokenRequest,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var tokenResponse = await APIClient.APICallAsync<TokenResponseModel>(HttpMethod.Post, APIEndpoint.UserAuthenticate,
                bodyContent: tokenRequest,
                cancellationToken: cancellationToken);

            APIClient.Token = tokenResponse.Token;

            return tokenResponse;
        }
    }
}
