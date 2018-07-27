namespace Game.Engine.Core
{
    using Game.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading;

    public class World : IDisposable
    {
        public List<Player> Players { get; } = new List<Player>();
        public List<Bullet> Bullets { get; } = new List<Bullet>();
        public List<GameObject> Objects { get; } = new List<GameObject>();
        public long Time { get; private set; } = 0;
        public long FrameNumber { get; private set; } = 0;
        public Vector2 WorldSize = new Vector2(6000, 6000);

        public Leaderboard Leaderboard { get; set; } = null;
        public bool IsLeaderboardNew = false;

        private readonly Timer heartbeat;
        private const int MS_PER_FRAME = 40;

        public World()
        {
            heartbeat = new Timer((state) =>
            {
                Step();
            }, null, 0, MS_PER_FRAME);
        }

        public void Step()
        {
            Time += MS_PER_FRAME;
            FrameNumber += 1;

            lock (Objects)
            {
                IsLeaderboardNew = false;
                if (FrameNumber % 20 == 0)
                {
                    Leaderboard = new Leaderboard
                    {
                        Entries = Players.Select(p => new Leaderboard.Entry
                            {
                                Name = p.Name,
                                Score = p.Score
                            })
                            .OrderByDescending(e => e.Score)
                            .ToList()
                    };
                    IsLeaderboardNew = true;
                }

                foreach (var player in Players.ToList())
                    player.Step(this);
                foreach (var bullet in Bullets.ToArray())
                    bullet.Step(this);

                foreach (var obj in Objects.ToList())
                {
                    obj.LastPosition = obj.Position;
                    obj.Position += obj.Momentum;

                    if (Math.Abs(obj.Position.X) > WorldSize.X / 2
                        || Math.Abs(obj.Position.Y) > WorldSize.Y / 2)
                    {
                        var newPosition = obj.Position;

                        if (newPosition.X > WorldSize.X / 2)
                            newPosition.X = WorldSize.X / -2;
                        if (newPosition.X < WorldSize.X / -2)
                            newPosition.X = WorldSize.X / 2;
                        if (newPosition.Y > WorldSize.Y / 2)
                            newPosition.Y = WorldSize.Y / -2;
                        if (newPosition.Y < WorldSize.Y / -2)
                            newPosition.Y = WorldSize.Y / 2;

                        obj.Position = newPosition;
                        obj.LastPosition = newPosition;
                    }
                }
                foreach (var player in Players.ToList())
                    player.SetupView(this);
            }
            // update some stuff.
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player);

            var r = new Random();

            player.GameObject = new GameObject
            {
                Position = new Vector2
                {
                    X = r.Next(-1000, 1000),
                    Y = r.Next(-1000, 1000)
                },
                Momentum = new Vector2
                {
                    X = 0,
                    Y = 0
                }
            };

            lock (Objects)
                Objects.Add(player.GameObject);
        }

        public void RemovePlayer(Player player)
        {
            Players.Remove(player);
            lock (Objects)
                Objects.Remove(player.GameObject);
        }

        public int PlayerCount
        {
            get
            {
                return Players.Count;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    heartbeat.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~World() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
