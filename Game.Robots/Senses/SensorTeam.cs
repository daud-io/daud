namespace Game.Robots.Senses
{
    using Game.API.Common;
    using Game.Robots.Models;
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

        public bool IsTeamMode { get => this.Robot.HookComputer.Hook.TeamMode; }

        public bool IsSameTeam(Fleet fleet)
            => IsSameTeam(fleet.Color);

        public bool IsSameTeam(string color)
        {
            if (!IsTeamMode)
                return false;

            return ParseTeam(color) == MyTeam;
        }

        public int MyTeamSize
        {
            get
            {
                return IsTeamMode
                    ? LeaderboardEntries(MyTeam).Count()
                    : 0;
            }
        }

        public int TheirTeamSize
        {
            get
            {
                return IsTeamMode
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
