namespace Game.Util.Commands
{
    using Game.Robots;
    using McMaster.Extensions.CommandLineUtils;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Subcommand("test", typeof(Test))]
    class PlayerCommand : CommandBase
    {
        class Test : CommandBase
        {
            protected async override Task ExecuteAsync()
            {
                var tasks = new List<Task>();

                for (int i = 0; i < 20; i++)
                {
                    var player = await API.Player.ConnectAsync();
                    var robot = new Robot(player);

                    tasks.Add(robot.Start());
                };

                await Task.WhenAll(tasks);
            }
        }
    }
}
