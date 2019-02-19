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
    [Subcommand("player", typeof(PlayerCommand))]
    [Subcommand("world", typeof(WorldCommand))]
    [Subcommand("registry", typeof(RegistryCommand))]
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

        [Option(Template = "--registry-server", Description = "full url of the Registry API server")]
        public string RegistryServer { get; }

        [Option(Template = "--registry-user-key", Description = "specify a UserKey for registry authentication")]
        public string RegistryUserKey { get; }

        [Option(Template = "--registry-password", Description = "specify a Password for registry authentication")]
        public string RegistryPassword { get; }

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

        private RegistryClient registryConnection = null;
        public RegistryClient RegistryConnection
        {
            get
            {
                if (registryConnection == null)
                    registryConnection = RegistryConnectAsync().Result;

                return registryConnection;
            }
        }

        public async Task<APIClient> ConnectAsync()
        {
            (var config, var context) = Configuration.Load(this);

            if (!Uri.TryCreate(Server ?? context.Uri ?? string.Empty, UriKind.Absolute, out Uri serverUri))
                throw new Exception("Config Server URI missing/invalid");

            var connection = new APIClient(serverUri);

            if (context.UserKey != null)
            {
                await connection.User.AuthenticateAsync(new UserIdentifier
                {
                    UserKey = UserKey ?? context.UserKey
                }, Password ?? context.Password);
            }

            return connection;
        }

        public async Task<RegistryClient> RegistryConnectAsync()
        {
            (var config, var context) = Configuration.Load(this);

            if (!Uri.TryCreate(RegistryServer ?? context.RegistryUri ?? string.Empty, UriKind.Absolute, out Uri serverUri))
                throw new Exception("Config Server URI missing/invalid");

            var connection = new RegistryClient(serverUri);

            if (context.RegistryUserKey == null)
                throw new Exception("Config missing UserKey");
            if (context.RegistryPassword == null)
                throw new Exception("Config missing Password");

            await connection.User.AuthenticateAsync(new UserIdentifier
            {
                UserKey = RegistryUserKey ?? context.RegistryUserKey
            }, RegistryPassword ?? context.RegistryPassword);

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
