namespace Game.Util.Commands
{
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Threading.Tasks;
    using TiledSharp;

    [Subcommand("shrink", typeof(Shrink))]
    [Subcommand("parse", typeof(Parse))]
    class WorldCommand : CommandBase
    {
        class Parse : CommandBase
        {
            [Argument(0)]
            public string File { get; set; } = null;

            protected override Task ExecuteAsync()
            {
                var map = new TmxMap(File);
                var tileset = map.Tilesets[0];

                
                foreach (var set in map.Tilesets)
                {
                    Console.WriteLine(set.Name);
                }


                return Task.FromResult(0);
            }
        }


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
