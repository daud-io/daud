namespace Game.API.Client
{
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.WebSockets;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class APIClient
    {
        private static HttpClient Client;

        public Uri BaseURL { get; private set; } = null;
        private string TokenInternal = null;

        public UserIdentifier UserIdentifier { get; private set; } = null;
        public string[] UserAccessIdentifiers { get; private set; } = null;
        public string UserAgent { get; set; }

        public Func<Task> OnSecurityException = null;

        static APIClient()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            Client.DefaultRequestHeaders.ExpectContinue = false;
        }

        public APIClient(Uri uri)
        {
            BaseURL = uri;

            UserAgent = Assembly.GetEntryAssembly()?.GetName()?.Name;
        }


        public async Task<HttpRequestMessage> RequestAsync<T>(
            HttpMethod method,
            string uri,
            object queryString,
            T content,
            IDictionary<string, string> headers = null)
        {
            var request = await RequestAsync(method, uri, queryString, headers);

            if (content is HttpContent)
                request.Content = content as HttpContent;
            else
                request.Content = new StringContent(JsonConvert.SerializeObject(content, Formatting.Indented));

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return request;
        }

        // lifted from here: http://geeklearning.io/serialize-an-object-to-an-url-encoded-string-in-csharp/
        // the idea is to turn an object graph into key/value pairs named and indexed in
        // a way that mvc.net will model bind propertly. This seems to fit the bill.
        public IDictionary<string, string> ToKeyValue(object metaToken)
        {
            if (metaToken == null)
                return null;

            JToken token = metaToken as JToken;
            if (token == null)
                return ToKeyValue(JObject.FromObject(metaToken));

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = ToKeyValue(child);
                    if (childContent != null)
                        contentData = contentData.Concat(childContent)
                            .ToDictionary(k => k.Key, v => v.Value);
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
                return null;

            var value = jValue?.Type == JTokenType.Date ?
                jValue?.ToString("o", CultureInfo.InvariantCulture) :
                jValue?.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }

        public Task<ClientWebSocket> ConnectWebSocket(
            APIEndpoint apiEndpoint,
            object querystring = null,
            Action<ClientWebSocket> configure = null,
            CancellationToken cancellationToken = default(CancellationToken))
            => ConnectWebSocket(apiEndpoint.Endpoint, querystring, configure, cancellationToken);

        public async Task<ClientWebSocket> ConnectWebSocket(
            string uri,
            object queryString = null,
            Action<ClientWebSocket> configure = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var webSocket = new ClientWebSocket();
            try
            {
                webSocket.Options.SetRequestHeader("Authorization", "Bearer " + Token);
                configure?.Invoke(webSocket);

                await webSocket.ConnectAsync(await RenderUriAsync(uri, queryString: queryString, websocket: true), cancellationToken);

                return webSocket;
            }
            catch (Exception)
            {
                try { webSocket.Dispose(); } catch (Exception) { }
                throw;
            }
        }

        private async Task<Uri> RenderUriAsync(string uri, object queryString = null, bool websocket = false)
        {
            if (queryString != null)
            {
                var keyValueContent = ToKeyValue(queryString);
                var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent);
                var urlEncodedString = await formUrlEncodedContent.ReadAsStringAsync();

                uri += "?" + urlEncodedString;
            }

            return new Uri(
                websocket
                ? new Uri(BaseURL.ToString()   // there's got to be a cleaner way to do this...
                    .Replace("http://", "ws://")
                    .Replace("https://", "wss://"))
                : BaseURL,
                uri
            );
        }

        public async Task<HttpRequestMessage> RequestAsync(
            HttpMethod method,
            string uri,
            object queryString = null,
            IDictionary<string, string> headers = null
        )
        {
            var request = new HttpRequestMessage(method, await RenderUriAsync(uri, queryString));

            request.Headers.Add("Authorization", "Bearer " + Token);

            if (UserAgent != null)
                request.Headers.Add("User-Agent", UserAgent);

            if (headers != null)
                foreach (var header in headers.Keys)
                    request.Headers.Add(header, headers[header]);

            return request;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
            => Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);


        public string ClientClaims { get; private set; }
        public string Token
        {
            get
            {
                return TokenInternal;
            }
            set
            {
                TokenInternal = value;

                if (value != null)
                {
                    var parsedJWT = new JwtSecurityTokenHandler().ReadJwtToken(value);

                    this.UserIdentifier = new UserIdentifier
                    {
                        UserKey = parsedJWT.Claims
                            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
                            ?.Value
                    };

                    this.UserAccessIdentifiers = parsedJWT.Claims
                        .FirstOrDefault(c => c.Type == SecurityTokenClaimKeys.SECURITY_IDENTIFIERS)
                        ?.Value
                        ?.Split(' ');

                    this.ClientClaims = parsedJWT.Claims
                        .FirstOrDefault(c => c.Type == SecurityTokenClaimKeys.CLIENT_CLAIMS)
                        ?.Value;
                }
                else
                {
                    this.UserAccessIdentifiers = null;
                    this.UserIdentifier = null;
                }
            }
        }

        public async Task<T> DefaultResponseHandler<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return await ReadAsAsync<T>(response);
            else
            {
                await EnsureSuccessStatus(response);

                // won't ever reach here.
                throw new Exception("Error in API Client");
            }
        }

        public async Task<T> APICallAsync<T>(
            HttpMethod method,
            APIEndpoint endpoint,
            object queryStringContent = null,
            object bodyContent = null,
            IDictionary<string, string> headers = null,
            Func<HttpResponseMessage, Task<T>> onResponse = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            using (var request = await RequestAsync(method, endpoint.Endpoint,
                queryStringContent,
                bodyContent,
                headers: headers))
            using (var response = await Client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            ))
            {
                if (onResponse != null)
                    return await onResponse.Invoke(response);

                return await DefaultResponseHandler<T>(response);
            }
        }

        public async Task EnsureSuccessStatus(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    if (OnSecurityException != null)
                        await OnSecurityException.Invoke();

                var wrapper = await ReadWrapper<string>(response);
                if (wrapper != null)
                    throw new Exception($"Server Returned Error: {(int)response.StatusCode} {response.StatusCode} {wrapper.ExceptionType}\n{wrapper.Exception}\n{wrapper.ExceptionStack}\n");
                else
                    response.EnsureSuccessStatusCode();
            }
        }

        internal async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
        {
            var wrapped = await ReadWrapper<T>(response);
            return wrapped.Response;
        }

        internal async Task<APIResponse<T>> ReadWrapper<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                var wrapped = JsonConvert.DeserializeObject<APIResponse<T>>(json);
                return wrapped;
            }
            catch (Exception)
            {
                throw new Exception($"Unexpected HTTP response (likely gateway): {json}");
            }
        }

        public UserMethods User { get => new UserMethods(this); }
        public ServerMethods Server { get => new ServerMethods(this); }
        public WorldMethods World { get => new WorldMethods(this); }
        public PlayerMethods Player { get => new PlayerMethods(this); }
    }
}
