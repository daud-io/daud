namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.API.Common.Models;
    using Game.Engine.Core.SystemActors;
    using Game.Engine.Core.SystemActors.CTF;
    using Game.Engine.Networking;
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

        public Leaderboard Leaderboard = null;

        private bool Processing = false;

        public Func<Fleet, Vector2> FleetSpawnPositionGenerator { get; set; }
        public Func<Leaderboard> LeaderboardGenerator { get; set; }
        public Func<Player, string, Fleet> NewFleetGenerator { get; set; }

        public int AdvertisedPlayerCount { get; set; }
        public string WorldKey { get; set; }
        public string Image { get; set; } = "default";

        public GameConfiguration GameConfiguration { get; set; }

        private uint _id = 0;
        public uint NextID()
        {
            lock (this)
                return _id++;
        }

        public World(Hook hook, GameConfiguration gameConfiguration)
        {
            this.GameConfiguration = gameConfiguration;
            OffsetTicks = DateTime.Now.Ticks;
            Hook = hook ?? Hook.Default;

            Console.WriteLine($"Initializing World: {this.Hook.Name}");

            InitializeSystemActors();
            InitializeStepTimer();
        }

        private void InitializeSystemActors()
        {
            InitializeSystemActor<SpawnLocationsActor>();
            InitializeSystemActor<WorldResizer>();
            InitializeSystemActor<LeaderboardActor>();
            InitializeSystemActor<Authenticator>();
            InitializeSystemActor<Advertisement>();
            InitializeSystemActor<RobotTender>();
            InitializeSystemActor<ObstacleTender>();
            InitializeSystemActor<CaptureTheFlag>();
            InitializeSystemActor<Sumo>();
            InitializeSystemActor<MapActor>();
        }

        public T GetActor<T>()
        {
            return this.Actors.OfType<T>().FirstOrDefault();
        }

        private void InitializeSystemActor<T>(T instance = null)
            where T : class, IActor, new()
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

                foreach (var body in Bodies)
                {
                    if (body.IsDirty)
                    {
                        body.DefinitionTime = this.Time;
                        body.OriginalPosition = body.Position;
                        body.OriginalAngle = body.Angle;
                        body.IsDirty = false;
                    }
                }

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

        public void BodyAdd(Body body)
        {
            Bodies.Add(body);
            if (body.IsStatic)
                RTreeStatic.Insert(body);
        }

        public void BodyRemove(Body body)
        {
            Bodies.Remove(body);
            if (body.IsStatic)
                RTreeStatic.Delete(body);
        }

        public float DistanceOutOfBounds(Vector2 position, int buffer = 0)
        {
            var pos = Vector2.Abs(position);
            return Math.Max(pos.X - Hook.WorldSize + buffer, Math.Max(pos.Y - Hook.WorldSize + buffer, 0));
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

        public IEnumerable<Body> BodiesNear(Vector2 point, int maximumDistance = 0)
        {
            if (maximumDistance == 0)
                return this.Bodies;
            else
            {
                var searchEnvelope = new Envelope(
                    point.X - maximumDistance / 2,
                    point.Y - maximumDistance / 2,
                    point.X + maximumDistance / 2,
                    point.Y + maximumDistance / 2
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

            return RandomPosition();
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
