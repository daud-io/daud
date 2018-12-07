namespace Game.Util
{
    using ConsoleTableExt;
    using Game.API.Client;
    using Game.Util.Commands;
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;

    [HelpOption("--help")]
    public abstract class CommandBase
    {
        public object Parent { get; } = null; // populated by commandline parser
        private CommandLineApplication app;

        protected virtual void Execute()
        {
            app.ShowHelp();
        }

        protected virtual Task ExecuteAsync()
        {
            try
            {
                Execute();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            return Task.FromResult(0);
        }

        protected async virtual Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            this.app = app;

            await ExecuteAsync();

            return 0;
        }

        public APIClient API
        {
            get
            {
                return Root.Connection;
            }
        }

        protected void Table(string name, object row)
        {
            Table(name, new[] { row });
        }

        private string Truncate(string s, int length)
        {
            if (s == null)
                return s;

            s = s.Replace("\r", "");
            s = s.Replace("\n", "\\n");

            if (s.Length > length)
                return s.Substring(0, length);
            else
                return s;
        }

        protected void Table(string name, IEnumerable<object> rows)
        {
            if (rows.Any())
            {
                var type = rows.First().GetType();
                var properties = type.GetProperties();

                var dt = new DataTable();
                foreach (var property in properties)
                    dt.Columns.Add(property.Name);

                foreach (var row in rows)
                    dt.Rows.Add(properties.Select(p => Truncate(p.GetValue(row)?.ToString(), 75)).ToArray());


                Console.WriteLine($"==== {name} ====");
                ConsoleTableBuilder
                   .From(dt)
                   .WithOptions(new ConsoleTableBuilderOption
                   {
                       Delimiter = " ",
                       TrimColumn = true
                   })
                   .WithFormat(ConsoleTableBuilderFormat.Minimal)
                   .ExportAndWriteLine();
            }
            else
                Console.WriteLine($"{name} has no data");
        }

        protected T GetParent<T>()
            where T : CommandBase
        {
            var o = this;
            while (o.Parent != null)
            {
                o = o.Parent as CommandBase;
                if (o is T)
                    return o as T;
            }

            throw new Exception("Cannot find requested parent");
        }

        public RootCommand Root
        {
            get
            {
                return GetParent<RootCommand>();
            }
        }
    }
}
