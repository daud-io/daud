namespace Game.Robots.Senses
{
    using Game.Engine.Networking.Client;
    using System.Collections.Generic;
    using System.Linq;

    public class SensorTeam : ISense
    {
        private readonly ContextRobot Robot;

        public enum Teams
        {
            Cyan,
            Red
        }

        public Teams MyTeam { get; private set; }
        public Teams TheirTeam { get; private set; }

        public SensorTeam(ContextRobot robot)
        {
            this.Robot = robot;
        }

        public void Sense()
        {
            this.MyTeam = ParseTeam(this.Robot.Color);
            if (MyTeam == Teams.Cyan)
                TheirTeam = Teams.Red;
            else
                TheirTeam = Teams.Cyan;
        }

        private Teams ParseTeam(string color)
        {
            if (color == "cyan")
                return Teams.Cyan;
            else
                return Teams.Red;
        }

        public int MyTeamSize
        {
            get
            {
                return this.Robot.HookComputer.TeamMode
                    ? LeaderboardEntries(MyTeam).Count()
                    : 0;
            }
        }

        public int TheirTeamSize
        {
            get
            {
                return this.Robot.HookComputer.TeamMode
                    ? LeaderboardEntries(TheirTeam).Count()
                    : 0;
            }
        }

        private IEnumerable<Leaderboard.Entry> LeaderboardEntries(Teams team)
        {
            var entries = this.Robot.Leaderboard.Entries;

            for (var i = 2; i < entries.Count; i++)
                if (ParseTeam(entries[i].Color) == team)
                    yield return entries[i];
        }
    }
}
