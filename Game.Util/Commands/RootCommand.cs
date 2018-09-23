namespace Game.Util.Commands
{
    using Game.API.Client;
    using Game.API.Common.Models;
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Threading.Tasks;

    [Command]
    [Subcommand("context", typeof(ContextCommand))]
    [Subcommand("server", typeof(ServerCommand))]
    public class RootCommand : CommandBase
    {

        [Option("--context", Description = "override the default, saved context and use the mentioned one")]
        public string UseContext { get; }

        [Option(Description = "full url of the Game API server")]
        public string Server { get; }

        [Option(Description = "specify a UserKey for authentication")]
        public string UserKey { get; }

        [Option(Description = "spefify a password for authentication")]
        public string Password { get; }

        [Option(Description = "Do not use auth token cached in context, reauthenticate instead")]
        public bool NoCachedToken { get; set; } = false;

        [Option(Description = "specify a token for authentication")]
        public string Token { get; }

        private APIClient connection = null;
        public APIClient Connection
        {
            get
            {
                if (connection == null)
                    connection = ConnectAsync().Result;

                return connection;
            }
        }

        public async Task<APIClient> ConnectAsync()
        {
            (var config, var context) = Configuration.Load(this);

            if (!Uri.TryCreate(Server ?? context.Uri ?? string.Empty, UriKind.Absolute, out Uri serverUri))
                throw new Exception("Config Server URI missing/invalid");

            var connection = new APIClient(serverUri);

            async Task authenticate()
            {
                var tokenResponse = await connection.User.AuthenticateAsync(new UserIdentifier
                {
                    UserKey = UserKey ?? context.UserKey
                }, Password ?? context.Password);

                if (!NoCachedToken)
                {
                    context.Token = tokenResponse.Token;
                    Configuration.Save(config);
                }
            }

            if (!string.IsNullOrEmpty(context.Token) && !NoCachedToken)
            {
                connection.Token = context.Token;
                connection.OnSecurityException = authenticate;
            }
            else
            {
                if (context.UserKey == null)
                    throw new Exception("Config missing UserKey");
                if (context.Password == null)
                    throw new Exception("Config missing Password");

                await authenticate();
                connection.OnSecurityException = authenticate;
            }

            return connection;
        }

        private class NullDisposable : IDisposable
        {
            void IDisposable.Dispose()
            {
            }
        }
    }
}
