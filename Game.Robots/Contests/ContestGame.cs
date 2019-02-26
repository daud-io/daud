namespace Game.Robots.Contests
{
    using Game.API.Client;
    using Game.API.Common.Models;
    using System;

    public class ContestGame : IDisposable
    {
        public string ArenaURL { get; set; }
        public APIClient API { get; set; }
        public Hook Hook { get; set; }

        public ConfigurableContextBot TestRobot { get; set; }
        public ConfigurableContextBot ChallengeRobot { get; set; }

        public void Dispose()
        {
                
        }
    }
}
