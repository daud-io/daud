namespace Game.Util
{
    using Game.Util.Commands;
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Threading.Tasks;

    public class Program
    {
        public async static Task<int> Main(string[] args)
        {
            try
            {
                return await CommandLineApplication.ExecuteAsync<RootCommand>(args);
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                    e = e.InnerException;

                Console.Error.WriteLine($"ERROR: {e}");
                return 1;
            }
        }
    }
}
