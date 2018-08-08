namespace Game.Engine.Core
{
    using Game.Engine.Core.Actors;
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
        public List<ActorBase> Actors { get; } = new List<ActorBase>();
        public List<Player> Players { get; } = new List<Player>();

        public List<GameObject> Objects { get; } = new List<GameObject>();

        public long Time { get; private set; } = 0;
        public long FrameNumber { get; private set; } = 0;
        public Vector2 WorldSize = new Vector2(6000, 6000);

        public Leaderboard Leaderboard { get; set; } = null;
        public bool IsLeaderboardNew = false;

        public Hook Hook { get; set; } = null;
        public bool IsHookNew = false;

        private readonly Timer heartbeat;
        private const int MS_PER_FRAME = 40;

        public World()
        {
            Hook = new Hook
            {
                BaseThrust = 6,
                HealthHitCost = 20,
                MaxBoostTime = 100,
                HealthRegenerationPerFrame = 0.3f,
                MaxSpeed = 12,
                MaxSpeedBoost = 40,
                MaxHealth = 100,
                ShootCooldownTime = 500,
                ShootCooldownTimeBot = 800,
                MaxHealthBot = 50,
                BaseThrustBot = 2,
                BulletLife = 2000,
                BulletSpeed = 50
            };

            heartbeat = new Timer((state) =>
            {
                //Console.WriteLine($"Frame: {FrameNumber}");
                Step();

                ConnectionHeartbeat.Step();
            }, null, 0, MS_PER_FRAME);

            var tender = new RobotTender();
            tender.Init(this);
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
                        Entries = Players
                            .Where(p => p.IsAlive)
                            .Select(p => new Leaderboard.Entry
                                {
                                    Name = p.Name,
                                    Score = p.Score
                                })
                                .OrderByDescending(e => e.Score)
                                .ToList()
                    };
                    IsLeaderboardNew = true;
                }

                foreach (var actor in Actors.ToList())
                    actor.PreStep();

                foreach (var actor in Actors.ToList())
                    actor.Step();

                foreach (var obj in Objects.ToList())
                {
                    obj.LastPosition = obj.Position;
                    obj.Position += obj.Momentum;

                    if (Math.Abs(obj.Position.X) > WorldSize.X / 2
                        || Math.Abs(obj.Position.Y) > WorldSize.Y / 2)
                    {
                        var newPosition = obj.Position;

                        if (newPosition.X > WorldSize.X / 2)
                            newPosition.X = (-WorldSize.X / 2) + (newPosition.X - WorldSize.X / 2);
                        if (newPosition.X < WorldSize.X / -2)
                            newPosition.X = (WorldSize.X / 2) + (newPosition.X + WorldSize.X / 2);
                        if (newPosition.Y > WorldSize.Y / 2)
                            newPosition.Y = (-WorldSize.Y / 2) + (newPosition.Y - WorldSize.Y / 2);
                        if (newPosition.Y < WorldSize.Y / -2)
                            newPosition.Y = (WorldSize.Y / 2) + (newPosition.Y + WorldSize.Y / 2);

                        obj.Position = newPosition;
                        obj.LastPosition = newPosition;
                    }
                }

                foreach (var actor in Actors.ToList())
                    actor.PostStep();
            }
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
                if (disposing)
                    heartbeat.Dispose();
                disposedValue = true;
        }
        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
