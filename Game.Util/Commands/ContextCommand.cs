namespace Game.Util.Commands
{
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    [Subcommand("get", typeof(Get))]
    [Subcommand("list", typeof(List))]
    [Subcommand("set", typeof(Set))]
    class ContextCommand : CommandBase
    {
        class Get : CommandBase
        {

            protected override void Execute()
            {
                (var all, var context) = Configuration.Load(this.Root);

                Console.WriteLine($"Context: {all.CurrentContext}");
                Console.WriteLine($"APIUri: {context.Uri}");
                Console.WriteLine($"UserKey: {context.UserKey}");
            }
        }

        class List : CommandBase
        {
            protected override void Execute()
            {
                (var all, var context) = Configuration.Load(this.Root);

                Table("Contexts", all.Contexts.Select(c => new
                {
                    IsCurrent = c.Key == all.CurrentContext ? "*" : "",
                    c.Key
                }));
            }
        }

        class Set : CommandBase
        {
            [Required, Argument(0, Description = "contextName")]
            public string ContextName { get; }

            protected override void Execute()
            {
                (var all, var context) = Configuration.Load(this.Root);

                if (!(all?.Contexts.Any(c => c.Key == ContextName) ?? false))
                    throw new Exception("Specific context does not exist in set of configured Contexts");

                all.CurrentContext = ContextName;

                Configuration.Save(all);
            }
        }
    }
}
