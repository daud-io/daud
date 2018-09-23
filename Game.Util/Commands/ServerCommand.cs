namespace Game.Util.Commands
{
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Threading.Tasks;

    [Subcommand("health", typeof(Health))]
    class ServerCommand : CommandBase
    {
        class Health : CommandBase
        {
            protected async override Task ExecuteAsync()
            {
                var started = DateTime.Now;

                var healthy = await API.Server.HealthGetAsync();

                Table("Health", new
                {
                    Server = API.BaseURL.ToString(),
                    ms = DateTime.Now.Subtract(started).TotalMilliseconds
                });
            }
        }
    }
}
