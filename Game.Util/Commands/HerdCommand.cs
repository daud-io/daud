namespace Game.Util.Commands
{
    using Game.Robots.Herding;
    using Game.Robots.Monsters;
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Threading.Tasks;

    class HerdCommand : CommandBase
    {
        [Option]
        public string World { get; set; } = "robo";

        [Option("--file")]
        public string File { get; set; } = null;

        protected async override Task ExecuteAsync()
        {
            Console.WriteLine("Starting Herd");


            var herd = new Shepherd(Root.Connection, "", worldKey: World);

            var parent = new SpawnBeast(herd);
            if (File != null)
                parent.Configure(File);

            await herd.StartRobot(parent);
            await parent.SpawnAsync();

            Console.ReadLine();
        }
    }
}