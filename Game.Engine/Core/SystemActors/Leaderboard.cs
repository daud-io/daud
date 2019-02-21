namespace Game.Engine.Core.SystemActors
{
    using Game.API.Common;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class LeaderboardActor : SystemActorBase
    {
        protected override void Cycle()
        {
            CycleMS = World.Hook.LeaderboardRefresh;

            if (World.LeaderboardGenerator != null)
                World.Leaderboard = World.LeaderboardGenerator();
            else
            {
                if (World.Hook.TeamMode)
                    World.Leaderboard = GenerateTeamLeaderboard();
                else
                    World.Leaderboard = GenerateStandardLeaderboard();
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
                ArenaRecord = World.Leaderboard?.ArenaRecord
                    ?? new Leaderboard.Entry()
            };

            var firstPlace = leaderboard.Entries.FirstOrDefault();
            if (World.Leaderboard != null && firstPlace?.Score > World.Leaderboard.ArenaRecord.Score)
                World.Leaderboard.ArenaRecord = firstPlace;

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