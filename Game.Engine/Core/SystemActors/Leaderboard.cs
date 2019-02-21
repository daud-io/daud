namespace Game.Engine.Core.SystemActors
{
    using Game.API.Common;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class LeaderboardActor : SystemActorBase
    {
        public Leaderboard Leaderboard = null;

        public LeaderboardActor()
        {
            CycleMS = World.Hook.LeaderboardRefresh;
        }

        protected override void Cycle()
        {
            if (World.LeaderboardGenerator != null)
                Leaderboard = World.LeaderboardGenerator();
            else
            {
                if (World.Hook.TeamMode)
                    Leaderboard = GenerateTeamLeaderboard();
                else
                    Leaderboard = GenerateStandardLeaderboard();
            }
        }

        protected Leaderboard GenerateStandardLeaderboard()
        {
            var leaderboard = new Leaderboard
            {
                Entries = Player.GetWorldPlayers(World)
                    .Where(p => p.IsAlive)
                    .Select(p => new Leaderboard.Entry
                    {
                        FleetID = p.Fleet?.ID ?? 0,
                        Name = p.Name,
                        Score = p.Score,
                        Color = p.Color,
                        Position = p.Fleet.FleetCenter,
                        Token = p.Token
                    })
                        .OrderByDescending(e => e.Score)
                        .ToList(),
                Type = "FFA",
                Time = World.Time,
                ArenaRecord = Leaderboard?.ArenaRecord
                    ?? new Leaderboard.Entry()
            };

            var firstPlace = leaderboard.Entries.FirstOrDefault();
            if (firstPlace?.Score > Leaderboard.ArenaRecord.Score)
                Leaderboard.ArenaRecord = firstPlace;

            return leaderboard;
        }

        protected Leaderboard GenerateTeamLeaderboard()
        {
            var entries = new List<Leaderboard.Entry>();

            var cyanTeam = Player.GetTeam(World, "cyan");
            var redTeam = Player.GetTeam(World, "red");

            entries.Add(new Leaderboard.Entry
            {
                Name = "cyan",
                Score = cyanTeam.Sum(p => p.Score),
                Color = "cyan"
            });

            entries.Add(new Leaderboard.Entry
            {
                Name = "red",
                Score = redTeam.Sum(p => p.Score),
                Color = "red"
            });

            entries.AddRange(Player.GetWorldPlayers(World)
                .Where(p => p.IsAlive)
                .OrderBy(p => p.Color)
                .ThenByDescending(p => p.Score)
                .Select(p => new Leaderboard.Entry
                {
                    FleetID = p.Fleet?.ID ?? 0,
                    Name = p.Name,
                    Score = p.Score,
                    Color = p.Color,
                    Position = p.Fleet?.FleetCenter ?? Vector2.Zero
                })
                .ToList());

            return new Leaderboard
            {
                Entries = entries,
                Type = "Team",
                Time = World.Time
            };
        }
    }
}