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
            [Option()]
            public int Replicas { get; set; } = 10;

            [Option()]
            public bool Firing { get; set; } = false;

            protected async override Task ExecuteAsync()
            {
                var tasks = new List<Task>();

                for (int i = 0; i < Replicas; i++)
                {
                    var player = await API.Player.ConnectAsync();
                    var robot = new Robot(player);
                    robot.AutoFire = Firing;

                    tasks.Add(robot.Start());
                };

                await Task.WhenAll(tasks);
            }
        }
    }
}
