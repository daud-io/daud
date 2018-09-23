namespace Game.Util
{
    using Game.Engine.Networking.Client;
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

        public GameConnection API
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

        protected void RenderPrivileges(string setFor, IDictionary<string, IDictionary<string, IEnumerable<ACLModel>>> privileges)
        {
            var rows = privileges.SelectMany(p =>
                    p.Value.SelectMany(v =>
                        v.Value.Select(a =>
                            new
                            {
                                @for = setFor,
                                right = v.Key,
                                key = a.OverrideKey,
                                identifiers = string.Join(" ", a.RequiredIdentifiers.OrderBy(i => i))
                            }
                        )
                    )
                )
                .OrderBy(r => r.@for)
                .ThenBy(r => r.right)
                .ThenBy(r => r.key);

            Table("Privileges",
                rows
            );

        }

        private string GetPart(string[] parts, int index)
        {
            return string.IsNullOrWhiteSpace(parts[index]) ? null : parts[index];
        }

        protected T GetIdentifier<T>(string identifier, params Action<T, string>[] map)
            where T : class, IIdentifier, new()
        {
            if (identifier == null)
                return null;

            var parts = identifier.Split('/');
            var model = new T();
            for (int i = map.Length - 1; i >= 0; i--)
            {
                var endOffset = (map.Length - 1) - i;
                var partIndex = (parts.Length-1) - endOffset;

                var part = (partIndex >= 0) && string.IsNullOrWhiteSpace(parts[partIndex]) ? null : parts[partIndex];
                map[i](model, part);

            }

            if (!model.IsValid)
                throw new Exception($"{typeof(T).Name} is invalid");

            return model;
        }


        protected T GetParent<T>()
            where T: CommandBase
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
