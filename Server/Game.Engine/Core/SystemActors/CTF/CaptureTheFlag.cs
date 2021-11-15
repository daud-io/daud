﻿namespace Game.Engine.Core.SystemActors.CTF
{
    using Game.API.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class CaptureTheFlag : SystemActorBase
    {
        private List<Flag> Flags = new List<Flag>();
        private List<Base> Bases = new List<Base>();
        private List<Team> Teams = new List<Team>();

        public uint GameRestartTime { get; set; } = 0;
        public uint GameEmptySince { get; set; } = 0;

        public long NextAnnounceTime { get; set; } = 0;

        public CaptureTheFlag(World world) : base(world)
        {
            CycleMS = 0;
            World.GetActor<SpawnLocationsActor>().GeneratorAdd("CTF", this.FleetSpawnPosition);
        }

        public Leaderboard LeaderboardGenerator()
        {
            var entries = Teams.Select(t => new Leaderboard.Entry
            {
                Color = t.ColorName,
                Name = t.ColorName,
                Score = t.Score,
                Position = t.Flag?.Position ?? Vector2.Zero,
                ModeData = new { flagStatus = t.Base.FlagIsHome() ? "Home" : "Taken" },
                FleetID = t.Flag.CarriedBy?.ID ?? 0
            }).ToList();

            var players = Player.GetWorldPlayers(World);

            foreach (var team in Teams)
            {
                entries.AddRange(players
                    .Where(p => p.Color == team.ColorName)
                    .Where(p => p.IsAlive)
                    .Select(p => new Leaderboard.Entry
                    {
                        FleetID = p.Fleet?.ID ?? 0,
                        Color = team.ColorName,
                        Name = p.Name,
                        Position = p.Fleet?.FleetCenter ?? Vector2.Zero,
                        Score = p.Score
                    }));
            }

            return new Leaderboard
            {
                Type = "CTF",
                Time = World.Time,
                Entries = entries
            };
        }

        public Vector2 FleetSpawnPosition(Fleet fleet)
        {
            const int POINTS_TO_TEST = 50;
            const int MAXIMUM_SEARCH_SIZE = 4000;

            var color = fleet?.Owner?.Color;

            if (color != null)
            {
                var team = Teams.FirstOrDefault(t => t.ColorName == color);
                if (team != null)
                {
                    var points = new List<Vector2>();
                    int failsafe = 10000;

                    while (points.Count < POINTS_TO_TEST)
                    {
                        var position = World.RandomPosition();
                        if (Vector2.Distance(position, team.BaseLocation) < World.Hook.CTFSpawnDistance)
                            points.Add(position);

                        if (failsafe-- < 0)
                            throw new Exception("Cannot find qualifying location in CTF Spawn");
                    }

                    return points.Select(p =>
                    {
                        var closeBodies = World.BodiesNear(p, MAXIMUM_SEARCH_SIZE)
                                .OfType<Ship>();
                        return new
                        {
                            Closest = closeBodies.Any()
                                ? closeBodies.Min(s => Vector2.Distance(s.Position, p))
                                : MAXIMUM_SEARCH_SIZE,
                            Point = p
                        };
                    })
                    .OrderByDescending(location => location.Closest)
                    .First().Point;
                }
            }

            return Vector2.Zero;
        }

        private void CreateTeam(string teamName, Sprites flagSprite, Vector2 basePosition)
        {
            var team = new Team
            {
                BaseLocation = basePosition,
                ColorName = teamName,
                CaptureTheFlag = this
            };

            Teams.Add(team);

            var b = new Base(World, this, basePosition, team);
            var flag = new Flag(World, flagSprite, team, b);
            b.Flag = flag;
            team.Flag = flag;
            team.Base = b;

            Flags.Add(flag);
            Bases.Add(b);

            flag.ReturnToBase();
        }

        public void TeamScored(Team team)
        {
            var players = Player.GetWorldPlayers(World);
            var message = (team.Score == 5)
                ? $"CTF: GAME OVER --- {team.ColorName} wins!!!"
                : $"CTF: {team.ColorName} scored!";

            foreach (var player in players)
                player?.SendMessage(message);

        }

        protected override void CycleThink()
        {
            if (GameRestartTime > 0 && World.Time > GameRestartTime)
            {
                GameRestartTime = 0;
                foreach (var team in Teams)
                {
                    team.Score = 0;
                    team.Flag.CarriedBy = null;
                    team.Flag.ReturnToBase();
                }

                foreach (var player in Player.GetWorldPlayers(World))
                {
                    player.Score = 0;
                }
            }

            foreach (var team in Teams)
            {
                if (team.Score >= 5 && GameRestartTime == 0)
                {
                    //var world = Worlds.Find("default");
                    //var players = Player.GetWorldPlayers(world);
                    //foreach (var player in players)
                    //    player.SendMessage("Next round of CTF (Capture the Flag) starts in 30 seconds, join now");

                    GameRestartTime = World.Time + 30000;
                }
            }

            var playerCount = Player.GetWorldPlayers(World)
                .Where(p => p.IsAlive).Count();

            if (playerCount == 0)
            {
                if (GameEmptySince == 0)
                    GameEmptySince = World.Time;
                else if (Teams.Any(t => t.Score > 0) && World.Time - GameEmptySince > 10000)
                    GameRestartTime = World.Time;
            }

            if (playerCount > 0)
                GameEmptySince = 0;


            if (World.Hook.CTFMode)
            {
                World.Hook.TeamMode = true;
            }

            if (World.Hook.CTFMode && Flags.Count == 0)
            {
                CreateTeam("cyan", Sprites.ctf_flag_blue, new Vector2(-World.Hook.WorldSize, World.Hook.WorldSize));
                CreateTeam("red", Sprites.ctf_flag_red, new Vector2(World.Hook.WorldSize, -World.Hook.WorldSize));
                World.FleetSpawnPositionGenerator = this.FleetSpawnPosition;
                World.LeaderboardGenerator = this.LeaderboardGenerator;
            }

            if (!World.Hook.CTFMode && Flags.Count > 0)
            {
                foreach (var flag in Flags)
                {
                    flag.Destroy();
                }

                foreach (var b in Bases)
                {
                    b.Destroy();
                }

                Flags.Clear();
                Bases.Clear();

                World.FleetSpawnPositionGenerator = null;
                World.LeaderboardGenerator = null;
            }
        }
    }
}
