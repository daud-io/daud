namespace Game.Engine.Core
{
    using Game.Engine.Core.Actors.Bots;
    using Game.Engine.Networking;
    using Game.Models;
    using Game.Models.Messages;
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
        public int WorldSize = 3000;

        public World()
        {
            Hook = Hook.Default;
            
            InitializeStepTimer();

            var tender = new RobotTender();
            tender.Init(this);

        }

        public void Step()
        {

            lock (this.Bodies)
            {
                Time = DateTime.Now.Ticks / 10000;

                foreach (var body in Bodies)
                {
                    body.Project(Time);
                    WrapAroundWorld(body);
                }

                foreach (var actor in Actors.ToArray())
                    actor.Step();

                foreach (var body in Bodies)
                    if (body.IsDirty)
                    {
                        body.DefinitionTime = this.Time;
                        body.OriginalPosition = body.Position;
                        body.IsDirty = false;
                    }

                ProcessLeaderboard();
            }
        }

        private void ProcessLeaderboard()
        {
            if (Time >= TimeLeaderboardRecalc)
            {
                Leaderboard = new Leaderboard
                {
                    Entries = Player.GetWorldPlayers(this)
                        .Where(p => p.IsAlive)
                        .Select(p => new Leaderboard.Entry
                        {
                            Name = p.Name,
                            Score = p.Score
                        })
                            .OrderByDescending(e => e.Score)
                            .ToList(),
                    Time = this.Time
                };
                TimeLeaderboardRecalc += 750;
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

        private long _id = 0;
        public long NextID()
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
                        .Where(b => (Vector2.Distance(b.Position, point) - b.Size) < maximumDistance)
                        .ToList();
                else
                    return this.Bodies
                        .Where(b => Vector2.Distance(b.Position, point) < maximumDistance)
                        .ToList();

            }
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
