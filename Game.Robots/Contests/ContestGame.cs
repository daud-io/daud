namespace Game.Robots.Contests
{
    using Game.API.Client;
    using Game.API.Common.Models;
    using System;
    using System.Threading.Tasks;

    public class ContestGame
    {
        public string ArenaURL { get; set; }
        public string WorldKey { get; set; }
        public APIClient API { get; set; }
        public Hook Hook { get; set; }

        public ConfigurableContextBot TestRobot { get; set; }
        public ConfigurableContextBot ChallengeRobot { get; set; }

        public async Task FinishedAsync()
        {
            await API.World.DeleteWorldAsync(WorldKey);
        }
    }
}
