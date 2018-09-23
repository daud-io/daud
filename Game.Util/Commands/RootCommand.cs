namespace Game.Util.Commands
{
    using Game.Engine.Networking.Client;
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Threading.Tasks;

    [Command]
    [Subcommand("server", typeof(ServerCommand))]
    public class RootCommand : CommandBase
    {

        [Option("--context", Description = "override the default, saved context and use the mentioned one")]
        public string UseContext { get; }

        [Option(Description = "full url of the Game API server")]
        public string Server { get; }

        [Option(Description = "specify a token for authentication")]
        public string Token { get; }

        private GameConnection connection = null;
        public GameConnection Connection
        {
            get
            {
                if (connection == null)
                    connection = ConnectAsync().Result;

                return connection;
            }
        }

        public async Task<GameConnection> ConnectAsync()
        {
            (var config, var context) = Configuration.Load(this);

            if (ImpersonateOrganizationKey != null)
                NoCachedToken = true;

            if (!Uri.TryCreate(Server ?? context.Uri ?? string.Empty, UriKind.Absolute, out Uri serverUri))
                throw new Exception("Config Server URI missing/invalid");

            var connection = new Connection(serverUri);

            if (Logging)
                connection.Logger = this;

            async Task authenticate()
            {
                var tokenResponse = await connection.User.AuthenticateAsync(new UserIdentifier
                {
                    OrganizationKey = OrganizationKey ?? context.OrganizationKey,
                    UserKey = UserKey ?? context.UserKey
                }, Password ?? context.Password);

                if (!NoCachedToken)
                {
                    context.Token = tokenResponse.Token;
                    Configuration.Save(config);
                }

                if (ImpersonateOrganizationKey != null)
                    await connection.User.ImpersonateAsync(new UserIdentifier(ImpersonateOrganizationKey, ImpersonateUserKey));
            }

            if (!string.IsNullOrEmpty(context.Token) && !NoCachedToken)
            {
                connection.Token = context.Token;
                connection.OnSecurityException = authenticate;
            }
            else
            {
                if (context.OrganizationKey == null)
                    throw new Exception("Config missing OrganizationKey");
                if (context.UserKey == null)
                    throw new Exception("Config missing UserKey");
                if (context.Password == null)
                    throw new Exception("Config missing Password");

                await authenticate();
                connection.OnSecurityException = authenticate;
            }

            return connection;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (this.Logging)
                Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return this.Logging;
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return new NullDisposable();
        }

        private class NullDisposable : IDisposable
        {
            void IDisposable.Dispose()
            {
            }
        }
    }
}
