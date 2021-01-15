namespace Game.Util.Commands
{
    using Game.Robots.Herding;
    using Game.Robots.Monsters;
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    class HerdCommand : CommandBase
    {
        [Option]
        public string World { get; set; } = "robo";

        [Option("--file")]
        public string File { get; set; } = null;

        [Option("--replicas")]
        public int Replicas { get; set; } = 1;

        protected async override Task ExecuteAsync()
        {
            Console.WriteLine("Starting Herd");


            var shepard = new Shepherd(Root.Connection, "", worldKey: World);
            var sheparedConfiguration = System.IO.File.ReadAllText(File);
            JsonConvert.PopulateObject(sheparedConfiguration, shepard);

            await shepard.RunAsync();

            Console.WriteLine("they dead, man.");
        }
    }
}