namespace Game.Robots.Contests
{
    using Game.API.Client;
    using Game.API.Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ContestGame
    {
        public string ArenaURL { get; set; }
        public string WorldKey { get; set; }
        public APIClient API { get; set; }
        public Hook Hook { get; set; }

        public List<ConfigurableContextBot> Robots = new List<ConfigurableContextBot>();

        public async Task FinishedAsync()
        {
            await API.World.DeleteWorldAsync(WorldKey);
        }

        public async static Task<ContestGame> CreateGameAsync(APIClient api, string hookUri)
        {
            var contest = new ContestGame();
            var worldKey = Guid.NewGuid().ToString();

            contest.Hook = Hook.Default;
            await UriTools.PatchAsync(hookUri, contest.Hook);

            contest.WorldKey = worldKey;
            contest.ArenaURL = (await api.World.PutWorldAsync(worldKey, contest.Hook));
            Console.WriteLine($"world create returned: {contest.ArenaURL}");
            contest.ArenaURL = "ws://" + contest.ArenaURL.Replace(worldKey, string.Empty);
            Console.WriteLine($"final: {contest.ArenaURL}");

            contest.API = new API.Client.APIClient(new Uri(contest.ArenaURL))
            {
                Token = api.Token
            };

            return contest;
        }
    }
}
