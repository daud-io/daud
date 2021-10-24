﻿namespace Game.Engine.Core.SystemActors
{
    using Game.API.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class LeaderboardActor : SystemActorBase
    {
        public LeaderboardActor(World world) : base(world)
        {

        }

        protected override void CycleThink()
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
                        Token = p.Token,
                        ModeData = new { advance = p.Advance }
                    })
                        .OrderByDescending(e => e.Score)
                        .ToList(),
                Type = "FFA",
                Time = World.Time,
                ArenaRecord = World.Leaderboard?.ArenaRecord
                    ?? new Leaderboard.Entry()
            };

            var firstPlace = leaderboard.Entries.FirstOrDefault();
            if (World.Leaderboard != null && firstPlace?.Score > World.Leaderboard?.ArenaRecord?.Score)
            {
                leaderboard.ArenaRecord = firstPlace;
                World.ArenaRecordResetTime = World.Time + 86400000;
                World.ArenaRecordHasReset = false;
            }


            if (World.Leaderboard != null && !World.ArenaRecordHasReset && World.Time >= World.ArenaRecordResetTime)
            {
                Console.WriteLine("Arena Record Score Reseting.");
                leaderboard.ArenaRecord.Score = 0;
                leaderboard.ArenaRecord.Name = "";
                leaderboard.ArenaRecord.FleetID = 0;
                World.ArenaRecordHasReset = true;
            }

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