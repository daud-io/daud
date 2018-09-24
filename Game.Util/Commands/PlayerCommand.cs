namespace Game.Util.Commands
{
    using McMaster.Extensions.CommandLineUtils;
    using System.Threading.Tasks;

    [Subcommand("test", typeof(Test))]
    class PlayerCommand : CommandBase
    {
        class Test : CommandBase
        {
            protected async override Task ExecuteAsync()
            {
                var player = await API.Player.ConnectAsync();
                await player.ListenAsync();
            }
        }
    }
}
