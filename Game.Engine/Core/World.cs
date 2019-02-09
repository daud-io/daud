namespace Game.Engine.Core
{
    using Game.API.Common.Models;
    using Game.Engine.Core.Maps;
    using Game.Engine.Networking;
    using Newtonsoft.Json;
    using RBush;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading;

    public class World : IDisposable
    {
        public uint Time { get; private set; } = 0;
        public uint LastStepSize { get; private set; } = 0;
        private readonly long OffsetTicks = 0;

        public Hook Hook { get; set; } = null;

        private Timer Heartbeat = null;

        private readonly List<IDisposable> Disposables = new List<IDisposable>();

        private RBush<Body> RTreeDynamic = new RBush<Body>();
        private RBush<Body> RTreeStatic = new RBush<Body>();

        public List<Body> Bodies = new List<Body>();
        public List<Group> Groups = new List<Group>();
        public List<IActor> Actors = new List<IActor>();

        private long TimeLeaderboardRecalc = 0;
        public Leaderboard Leaderboard = null;

        private int SearchMargin = 0;

        private bool Processing = false;

        public Func<Fleet, Vector2> FleetSpawnPositionGenerator { get; set; }
        public Func<Leaderboard> LeaderboardGenerator { get; set; }
        public Func<Player, string, Fleet> NewFleetGenerator { get; set; }


        public int AdvertisedPlayerCount {get;set;}
        public string WorldKey { get; set; }

        public string Image { get; set; } = "default";
        public MapActor MapActor { get; private set; } = null;

        public World(Hook hook = null)
        {
            OffsetTicks = DateTime.Now.Ticks;
            Hook = hook ?? Hook.Default;

            SystemActor<Advertisement>();
            SystemActor<RobotTender>();
            SystemActor<ObstacleTender>();
            SystemActor<CaptureTheFlag>();
            SystemActor<Sumo>();

            SystemActor(MapActor = new MapActor());

            Console.WriteLine($"Initializing World: {this.Hook.Name}");

            InitializeStepTimer();
        }

        private void SystemActor<T>(T instance = null)
            where T: class, IActor, new()
        {
            var actor = instance ?? new T();
            actor.Init(this);
        }

        public void Step()
        {
            if (Processing)
                return;

            Processing = true;
            lock (this.Bodies)
            {
                var start = DateTime.Now;

                var oldTime = Time;
                Time = (uint)((start.Ticks - OffsetTicks) / 10000);
                LastStepSize = Time - oldTime;

                RTreeDynamic.Clear();
                foreach (var body in Bodies)
                {
                    body.Project(Time);
                    body.Envelope = new Envelope(
                        body.Position.X - body.Size,
                        body.Position.Y - body.Size,
                        body.Position.X + body.Size,
                        body.Position.Y + body.Size);
                }

                RTreeDynamic.BulkLoad(Bodies.Where(b => !b.IsStatic));

                var origActors = Actors.ToList();
                foreach (var actor in Actors)
                {
                    int actors = Actors.Count();
                    actor.Think();
                    if (Actors.Count() != actors)
                        throw new Exception("Collection modified in think time");
                }

                foreach (var actor in Actors.ToList())
                    actor.CreateDestroy();

                var unindexed = 0;
                var indexed = 0;
                foreach (var body in Bodies)
                {
                    if (body.IsDirty || !body.Indexed)
                    {
                        body.DefinitionTime = this.Time;
                        body.OriginalPosition = body.Position;
                        body.OriginalAngle = body.Angle;
                        body.IsDirty = false;

                        /*if (Vector2.Distance(body.IndexedPosition, body.Position) > SearchMargin)
                        {
                            BodyCleaned(body);
                            indexed++;
                        }
                        else
                            unindexed++;*/
                    }
                }

                if (false && unindexed + indexed > 0)
                    Console.WriteLine($"{1f * indexed / (unindexed + indexed)}\t");

                ProcessLeaderboard();

                if (Time > 10000) // lets not get too excited if things slow down when initialized
                {
                    var elapsed = DateTime.Now.Subtract(start).TotalMilliseconds;
                    if (elapsed > Hook.StepTime)
                        Console.WriteLine($"**** 100% processing time warning: {elapsed}");
                    else if (elapsed > Hook.StepTime * 0.8f)
                        Console.WriteLine($"*** 80% processing time warning: {elapsed}");
                    else if (elapsed > Hook.StepTime * 0.5f)
                        Console.WriteLine($"** 50% processing time warning: {elapsed}");
                }
            }
            Processing = false;
        }

        public void BodyCleaned(Body body)
        {
            if (body.IsStatic)
                RTreeStatic.Delete(body);
            else
                RTreeDynamic.Delete(body);

            if (!body.Exists)
                return;

            body.Envelope = new Envelope(
                body.Position.X - body.Size, 
                body.Position.Y - body.Size, 
                body.Position.X + body.Size, 
                body.Position.Y + body.Size);

            body.IndexedPosition = body.Position;
            body.Indexed = true;
            body.Updated = true;

            if (body.IsStatic)
                RTreeStatic.Insert(body);
            else
                RTreeDynamic.Insert(body);
        }

        public void BodyAdd(Body body)
        {
            Bodies.Add(body);
        }

        public void BodyRemove(Body body)
        {
            Bodies.Remove(body);
            return;

            if (body.IsStatic)
                RTreeStatic.Delete(body);
            else
                RTreeDynamic.Delete(body);
            body.Removed = true;
        }

        public float DistanceOutOfBounds(Vector2 position, int buffer = 0)
        {
            var pos = Vector2.Abs(position);
            return Math.Max(pos.X - Hook.WorldSize + buffer, Math.Max(pos.Y - Hook.WorldSize + buffer, 0));
        }

        private void ProcessLeaderboard()
        {
            if (Time >= TimeLeaderboardRecalc)
            {
                if (LeaderboardGenerator != null)
                    Leaderboard = LeaderboardGenerator();
                else
                {
                    if (Hook.TeamMode)
                    {
                        var entries = new List<Leaderboard.Entry>();

                        var cyanTeam = Player.GetTeam(this, "cyan");
                        var redTeam = Player.GetTeam(this, "red");

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

                        entries.AddRange(Player.GetWorldPlayers(this)
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

                        Leaderboard = new Leaderboard
                        {
                            Entries = entries,
                            Type = "Team",
                            Time = this.Time
                        };
                    }
                    else
                    {
                        Leaderboard = new Leaderboard
                        {
                            Entries = Player.GetWorldPlayers(this)
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
                                    .Take(10)
                                    .ToList(),
                            Type = "FFA",
                            Time = this.Time,
                            ArenaRecord = Leaderboard?.ArenaRecord
                                ?? new Leaderboard.Entry()
                        };
						Hook.WorldSize = Hook.WorldSizeBasic + this.AdvertisedPlayerCount * Hook.WorldAreaDeltaPerPlayer;

                        var firstPlace = Leaderboard.Entries.FirstOrDefault();
                        if (firstPlace?.Score > Leaderboard.ArenaRecord.Score)
                            Leaderboard.ArenaRecord = firstPlace;
                    }
                }

                TimeLeaderboardRecalc = this.Time + Hook.LeaderboardRefresh;
            }
        }

        private void WrapAroundWorld(Body body)
        {
            var position = body.Position;

            if (position.X > Hook.WorldSize)
                position.X -= 2 * Hook.WorldSize;
            if (position.X < -Hook.WorldSize)
                position.X += 2 * Hook.WorldSize;
            if (position.Y > Hook.WorldSize)
                position.Y -= 2 * Hook.WorldSize;
            if (position.Y < -Hook.WorldSize)
                position.Y += 2 * Hook.WorldSize;

            if (position.X != body.Position.X
                || position.Y != body.Position.Y)
                body.Position = position;
        }

        private void InitializeStepTimer()
        {
            Heartbeat = new Timer((state) =>
            {
                if (Processing)
                    return;

                Step();
                ConnectionHeartbeat.Step();

            }, null, 0, Hook.StepTime);
            Disposables.Add(Heartbeat);
        }

        private uint _id = 0;
        public uint NextID()
        {
            lock(this)
                return _id++;
        }

        public IEnumerable<Body> BodiesNear(Vector2 point, int maximumDistance = 0)
        {
            if (maximumDistance == 0)
                return this.Bodies;
            else
            {
                var searchEnvelope = new Envelope(
                    point.X - maximumDistance / 2 - SearchMargin,
                    point.Y - maximumDistance / 2 - SearchMargin,
                    point.X + maximumDistance / 2 + SearchMargin,
                    point.Y + maximumDistance / 2 + SearchMargin
                );

                return RTreeDynamic.Search(searchEnvelope)
                    .Union(RTreeStatic.Search(searchEnvelope));
            }
        }

        public Vector2 RandomPosition()
        {
            var r = new Random();
            return new Vector2
            {
                X = r.Next(-Hook.WorldSize, Hook.WorldSize),
                Y = r.Next(-Hook.WorldSize, Hook.WorldSize)
            };
        }

        public Vector2 RandomSpawnPosition(Fleet fleet = null)
        {
            if (FleetSpawnPositionGenerator != null)
                return FleetSpawnPositionGenerator(fleet);

            var r = new Random();

            switch (Hook.SpawnLocationMode)
            {
                default:
                case "Corners":
                    var x = r.NextDouble() > .5
                        ? 1
                        : -1;
                    var y = r.NextDouble() > .5
                        ? 1
                        : -1;

                    return new Vector2
                    {
                        X = x * Hook.WorldSize * 0.95f,
                        Y = y * Hook.WorldSize * 0.95f
                    };

                case "Static":
                    return Hook.SpawnLocation;

                case "QuietSpot":
                    const int POINTS_TO_TEST = 10;
                    const int MAXIMUM_SEARCH_SIZE = 4000;

                    var points = new List<Vector2>();

                    for (var i = 0; i < POINTS_TO_TEST; i++)
                        points.Add(RandomPosition());

                    return points.Select(p =>
                    {
                        var closeBodies = BodiesNear(p, MAXIMUM_SEARCH_SIZE)
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
                if (disposing)
                {

                    foreach (var player in Player.GetWorldPlayers(this))
                        try
                        {
                            player.Destroy();
                        }
                        catch (Exception) { }

                    foreach (var d in Disposables)
                        try
                        {
                            d.Dispose();
                        }
                        catch (Exception) { }
                }
            disposedValue = true;
        }
        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
