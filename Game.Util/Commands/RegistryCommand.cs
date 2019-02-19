namespace Game.Util.Commands
{
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    [Subcommand("list", typeof(List))]
    class RegistryCommand : CommandBase
    {
        class List : CommandBase
        {
            protected async override Task ExecuteAsync()
            {
                var list = await RegistryAPI.Registry.ListAsync();

                Console.WriteLine(JsonConvert.SerializeObject(list));
            }
        }
    }
}
