namespace Game.Engine.Core
{
    using Game.Engine.Networking;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading;

    public class World : IDisposable
    {
        public long Time { get; private set; } = 0;

        public Hook Hook { get; set; } = null;

        private Timer Heartbeat = null;

        private readonly List<IDisposable> Disposables = new List<IDisposable>();

        public List<ProjectedBody> Bodies = new List<ProjectedBody>();
        public List<IActor> Actors = new List<IActor>();

        private long TimeLeaderboardRecalc = 0;
        public Leaderboard Leaderboard = null;
        public int WorldSize = 6000;

        public World()
        {
            Hook = Hook.Default;
            
            InitializeStepTimer();

            var robotTender = new RobotTender();
            robotTender.Init(this);

            var objstacleTender = new ObstacleTender();
            objstacleTender.Init(this);

        }

        public void Step()
        {
            lock (this.Bodies)
            {
                var start = DateTime.Now;
                Time = start.Ticks / 10000;

                foreach (var body in Bodies.ToList())
                {
                    if (body != null)
                        body.Project(Time);

                    //WrapAroundWorld(body);
                }

                foreach (var actor in Actors.ToList())
                    if (actor != null)
                        actor.Step();

                foreach (var body in Bodies)
                    if (body.IsDirty)
                    {
                        body.DefinitionTime = this.Time;
                        body.OriginalPosition = body.Position;
                        body.OriginalAngle = body.Angle;
                        body.IsDirty = false;
                    }

                ProcessLeaderboard();

                var elapsed = DateTime.Now.Subtract(start).TotalMilliseconds;
                if (elapsed > Hook.StepTime * 0.5f)
                    Console.WriteLine("50% processing time warning");
            }
        }

        public float DistanceOutOfBounds(Vector2 position)
        {
            var pos = Vector2.Abs(position);

            return Math.Max(pos.X - WorldSize, Math.Max(pos.Y - WorldSize, 0));
        }

        private void ProcessLeaderboard()
        {
            if (Time >= TimeLeaderboardRecalc)
            {
                if (Hook.TeamMode)
                {
                    Leaderboard = new Leaderboard
                    {
                        Entries = Player.GetWorldPlayers(this)
                            .Where(p => p.IsAlive)
                            .GroupBy(p => p.Fleet.Color)
                            .Select(g => new Leaderboard.Entry
                            {
                                Name = g.Key,
                                Score = g.Sum(p => p.Score),
                                Color = g.Key ?? "white"
                            })
                                .OrderByDescending(e => e.Score)
                                .Take(10)
                                .ToList(),
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
                                Name = p.Name,
                                Score = p.Score,
                                Color = p.Fleet?.Color ?? "white",
                                Position = p.Fleet.Position
                            })
                                .OrderByDescending(e => e.Score)
                                .Take(10)
                                .ToList(),
                        Type = "FFA",
                        Time = this.Time,
                        ArenaRecord = Leaderboard?.ArenaRecord
                            ?? new Leaderboard.Entry()
                    };

                    var firstPlace = Leaderboard.Entries.FirstOrDefault();
                    if (firstPlace?.Score > Leaderboard.ArenaRecord.Score)
                        Leaderboard.ArenaRecord = firstPlace;
                }

                TimeLeaderboardRecalc = this.Time + Hook.LeaderboardRefresh;
            }
        }

        private void WrapAroundWorld(ProjectedBody body)
        {
            var position = body.Position;

            if (position.X > WorldSize)
                position.X -= 2 * WorldSize;
            if (position.X < -WorldSize)
                position.X += 2 * WorldSize;
            if (position.Y > WorldSize)
                position.Y -= 2 * WorldSize;
            if (position.Y < -WorldSize)
                position.Y += 2 * WorldSize;

            if (position.X != body.Position.X
                || position.Y != body.Position.Y)
                body.Position = position;
        }

        private void InitializeStepTimer()
        {
            Heartbeat = new Timer((state) =>
            {
                Step();
                ConnectionHeartbeat.Step();

            }, null, 0, Hook.StepTime);
            Disposables.Add(Heartbeat);
        }

        private int _id = 0;
        public int NextID()
        {
            return _id++;
        }

        public IEnumerable<ProjectedBody> BodiesNear(Vector2 point, int maximumDistance = 0, bool offsetSize = false)
        {
            if (maximumDistance == 0)
                return this.Bodies
                    .ToList();
            else
            {
                if (offsetSize)
                    return this.Bodies
                        .Where(b => b != null)
                        .Where(b => (Vector2.Distance(b.Position, point) - b.Size) < maximumDistance)
                        .ToList();
                else
                    return this.Bodies
                        .Where(b => b != null)
                        .Where(b => Vector2.Distance(b.Position, point) < maximumDistance)
                        .ToList();

            }
        }

        public Vector2 RandomPosition()
        {
            var r = new Random();
            return new Vector2
            {
                X = r.Next(-WorldSize, WorldSize),
                Y = r.Next(-WorldSize, WorldSize)
            };
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
                if (disposing)
                    foreach (var d in Disposables)
                        d.Dispose();
            disposedValue = true;
        }
        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
