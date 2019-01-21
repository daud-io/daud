namespace Game.Robots.Senses
{
    using Game.Engine.Networking.Client;
    using Newtonsoft.Json;
    using System.Numerics;

    public class SensorCTF : ISense
    {
        public class CTFTeam
        {
            public SensorTeam.Teams Team { get; set; }
            public Vector2 BasePosition { get; set; }
            public bool FlagIsHome { get; set; }
            public Vector2 FlagPosition { get; set; }
            public uint FlagCarriedBy { get; set; }
            public int Score { get; set; }
        }

        private readonly ContextRobot Robot;
        private Leaderboard Leaderboard = null;

        public bool CTFModeEnabled { get; private set; }

        public CTFTeam OurTeam { get; set; }
        public CTFTeam TheirTeam { get; set; }

        public SensorCTF(ContextRobot robot)
        {
            this.Robot = robot;
        }


        public bool IsCarryingFlag
        {
            get
            {
                return this.Robot.FleetID == TheirTeam.FlagCarriedBy;
            }
        }

        private CTFTeam EvaluateTeam(SensorTeam.Teams team)
        {
            var entry = Leaderboard.Entries[team == SensorTeam.Teams.Cyan
                ? 0
                : 1];

            var modeData = entry.ModeData;
            var ctfData = JsonConvert.DeserializeAnonymousType(modeData, new { flagStatus = "" });

            var ctfTeam = new CTFTeam
            {
                Team = team,
                BasePosition = team == SensorTeam.Teams.Cyan
                    ? new Vector2(-this.Robot.WorldSize, -this.Robot.WorldSize)
                    : new Vector2(+this.Robot.WorldSize, +this.Robot.WorldSize),
                FlagIsHome = ctfData.flagStatus == "Home",
                FlagPosition = entry.Position,
                FlagCarriedBy = entry.FleetID,
                Score = entry.Score
            };

            return ctfTeam;
        }

        public void Sense()
        {
            if (Leaderboard != this.Robot.Leaderboard)
            {
                Leaderboard = this.Robot.Leaderboard;
                EvaluateLeaderboard();
            }
        }

        private void EvaluateLeaderboard()
        {
            this.CTFModeEnabled = this.Leaderboard.Type == "CTF";

            if (this.CTFModeEnabled && Leaderboard.Entries.Count > 1)
            {
                var cyan = EvaluateTeam(SensorTeam.Teams.Cyan);
                var red = EvaluateTeam(SensorTeam.Teams.Red);

                if (this.Robot.SensorTeam.Team == SensorTeam.Teams.Cyan)
                {
                    OurTeam = cyan;
                    TheirTeam = red;
                }
                else
                {
                    OurTeam = red;
                    TheirTeam = cyan;
                }
            }
        }
    }
}
