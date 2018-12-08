namespace Game.Util.Commands
{
    using McMaster.Extensions.CommandLineUtils;
    using System.Threading.Tasks;

    [Subcommand("shrink", typeof(Shrink))]
    class World : CommandBase
    {
        class Shrink : CommandBase
        {
            [Option]
            public string World { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                for (int i = 4200; i > 0; i -= 10)
                {
                    await API.Server.HookAsync(new { WorldSize = i }, World);
                    await Task.Delay(100);
                }
            }
        }
    }
}
