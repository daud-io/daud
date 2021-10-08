namespace Game.Util.Commands
{
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    [Subcommand(typeof(Shrink))]
    [Subcommand(typeof(Hook))]
    [Subcommand(typeof(Create))]
    [Subcommand(typeof(Delete))]
    [Subcommand(typeof(Reset))]
    [Command("world")]
    class WorldCommand : CommandBase
    {
        [Command("create")]
        class Create : CommandBase
        {
            [Argument(0)]
            public string World { get; set; }

            [Argument(1)]
            public string HookJSON { get; set; } = null;

            [Option]
            public string File { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                var hook = JsonConvert.DeserializeObject(HookJSON ?? System.IO.File.ReadAllText(File));
                hook = await API.World.PutWorldAsync(World, hook);

                Console.WriteLine(hook);
            }
        }

        [Command("delete")]
        class Delete : CommandBase
        {
            [Argument(0)]
            public string WorldKey { get; set; }

            protected async override Task ExecuteAsync()
            {
                await API.World.DeleteWorldAsync(WorldKey);
            }
        }

        [Command("reset")]
        class Reset : CommandBase
        {
            [Argument(0)]
            public string WorldKey { get; set; }

            protected async override Task ExecuteAsync()
            {
                await API.World.ResetWorldAsync(WorldKey);
            }
        }

        [Command("hook")]
        class Hook : CommandBase
        {
            [Argument(0)]
            public string World { get; set; }

            [Argument(1)]
            public string HookJSON { get; set; } = null;

            [Option]
            public string File { get; set; } = null;

            [Option("--default")]
            public bool Default { get; set; } = false;


            [Option("--url")]
            public string Url { get; set; } = null;

            protected async override Task ExecuteAsync()
            {

                if (Url != null)
                    using (WebClient cln = new WebClient())
                        HookJSON = await cln.DownloadStringTaskAsync(Url);

                var hook = JsonConvert.DeserializeObject(HookJSON ?? System.IO.File.ReadAllText(File));
                hook = await API.World.PostHookAsync(hook, World);

                Console.WriteLine(hook);
            }
        }

        [Command("shrink")]
        class Shrink : CommandBase
        {
            [Option]
            public string World { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                for (int i = 4200; i > 0; i -= 10)
                {
                    await API.World.PostHookAsync(new { WorldSize = i }, World);
                    await Task.Delay(100);
                }
            }
        }
    }
}
