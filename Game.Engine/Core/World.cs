﻿namespace Game.Engine.Core
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using BepuUtilities.Memory;
    using Game.API.Common;
    using Game.API.Common.Models;
    using Game.Engine.Core.Scoring;
    using Game.Engine.Core.SystemActors;
    using Game.Engine.Core.SystemActors.CTF;
    using Game.Engine.Core.SystemActors.Royale;
    using Game.Engine.Networking;
    using Game.Engine.Physics;
    using RBush;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading;

    public class World : IDisposable
    {
        public string WorldKey { get; set; }
        public string GameID { get; set; }

        public int AdvertisedPlayerCount { get; set; }
        public int SpectatorCount { get; set; }

        // the canonical game time, in milliseconds, from world start
        public uint Time { get; private set; } = 0;
        // offset between system clock and world start
        private readonly long OffsetTicks = 0;

        public Hook Hook { get; set; } = null;

        // spatial indices (RTree)
        // collectively these contain all the bodies in this world

        // the dynamic rtree is cleared and rebuilt every cycle
        private RBush<Body> RTreeDynamic = new RBush<Body>();
        // the static rtree is updated incrementally (originally for map tiles)
        private RBush<Body> RTreeStatic = new RBush<Body>();


        // lists of bodies, groups in the world
        public List<Body> Bodies = new List<Body>();
        public List<Group> Groups = new List<Group>();

        // list of IActors in the world. Actors are things that think.
        public List<IActor> Actors = new List<IActor>();

        // timer for world step entry
        private Timer Heartbeat = null;

        // most recent leaderboard available
        public Leaderboard Leaderboard = null;
        public bool ArenaRecordHasReset { get; set; } = false;
        public long ArenaRecordResetTime { get; set; } = 0;

        public Func<Fleet, Vector2> FleetSpawnPositionGenerator { get; set; }
        public Func<Leaderboard> LeaderboardGenerator { get; set; }
        public Func<Player, string, Fleet> NewFleetGenerator { get; set; }

        public GameConfiguration GameConfiguration { get; set; }

        public ScoringBase Scoring = new DefaultScoring();

        private uint LastObjectID = 0;
        public uint GenerateObjectID() { lock (this) return ++LastObjectID; }

        public bool CanSpawn { get => Hook.CanSpawn; set => Hook.CanSpawn = value; }
        public string CanSpawnReason { get; set; }

        private BufferPool bufferPool = new BufferPool();
        private Simulation Simulation = null;
        

        public World(Hook hook, GameConfiguration gameConfiguration)
        {
            this.GameConfiguration = gameConfiguration;
            OffsetTicks = DateTime.Now.Ticks;
            Hook = hook ?? Hook.Default;
            GameID = Guid.NewGuid().ToString().Replace("-", "");

            Console.WriteLine($"Initializing World: {this.Hook.Name}");

            this.Simulation = Simulation.Create(bufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, -10, 0)), new PositionLastTimestepper());
            this.Simulation.Statics.Add(new StaticDescription(new Vector3(0, -10, 0), new CollidableDescription(this.Simulation.Shapes.Add(new Box(100000, 10, 100000)), 0.1f)));

            InitializeSystemActors();
            InitializeStepTimer();
        }

        // main entry to the world. This will be called every Hook.StepSize milliseconds
        public void Step()
        {
            lock (this.Bodies)
            {
                var start = DateTime.Now;
                // calculate the new game time
                Time = (uint)((start.Ticks - OffsetTicks) / 10000);


                this.Simulation.Timestep(1000f/Hook.StepTime);

                // the dynamic index is rebuilt each step
                RebuildDynamicIndex();

                // every registered actor gets a chance to think
                ActorsThink();
                // every registered actor gets a chance to create and destroy new bodies
                ActorsCreateDestroy();

                // any bodies that were dirtied, need to be updated
                UpdateDirtyBodies();

                CheckTimings(start);
            }
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
            InitializeSystemActor<TeamColors>();
            InitializeSystemActor<RoomReset>();
            InitializeSystemActor<RoyaleMode>();
            InitializeSystemActor<AdvanceRetreat>();
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

        private void CheckTimings(DateTime start)
        {
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

        private void UpdateDirtyBodies()
        {
            foreach (var body in Bodies)
            {
                var reference = Simulation.Bodies.GetBodyReference(body.BodyHandle);
                body.UpdateBodyReference(reference, Time);
            }
        }

        private void ActorsCreateDestroy()
        {
            foreach (var actor in Actors.ToList())
                actor.CreateDestroy();
        }

        private void ActorsThink()
        {
            foreach (var actor in Actors)
            {
                int actors = Actors.Count();
                actor.Think();
                if (Actors.Count() != actors)
                    throw new Exception($"Collection modified in think time by {actor.GetType().Name}");
            }
        }

        private void RebuildDynamicIndex()
        {
            RTreeDynamic.Clear();
            foreach (var body in Bodies)
            {
                var reference = Simulation.Bodies.GetBodyReference(body.BodyHandle);
                body.UpdateFromBodyReference(reference, Time);

                body.Envelope = new Envelope(
                    body.Position.X - body.Size,
                    body.Position.Y - body.Size,
                    body.Position.X + body.Size,
                    body.Position.Y + body.Size);
            }

            RTreeDynamic.BulkLoad(Bodies.Where(b => !b.IsStatic));
        }

        public IEnumerable<Body> BodiesNear(Envelope searchArea)
        {
            // https://forum.bepuentertainment.com/viewtopic.php?f=4&t=2720&p=15103&hilit=query#p15103
            
            return RTreeDynamic.Search(searchArea)
                    .Union(RTreeStatic.Search(searchArea));
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
                return BodiesNear(searchEnvelope);
            }
        }

        public void BodyAdd(Body body)
        {
            Bodies.Add(body);
            if (body.IsStatic)
                RTreeStatic.Insert(body);

            int size = Math.Min(Math.Max(body.Size, 1), 1000);
            var capsule = new Capsule(size, 5000);
            var mass = 4/3 * MathF.PI * (size ^ 3) * 0.25f;
            capsule.ComputeInertia(mass, out var capsuleIntertia);
            capsuleIntertia.InverseInertiaTensor.XX = 0;
            capsuleIntertia.InverseInertiaTensor.YY = 0;

            body.BodyHandle = Simulation.Bodies.Add(BodyDescription.CreateDynamic(
                new Vector3(body.Position.X, 0, body.Position.Y), 
                capsuleIntertia,
                new CollidableDescription(this.Simulation.Shapes.Add(capsule), 0.1f), 
                new BodyActivityDescription(0.01f)
            ));
        }

        public void BodyRemove(Body body)
        {
            Simulation.Bodies.Remove(body.BodyHandle);
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
                Step();
                ConnectionHeartbeat.Step();
            }, null, 0, Hook.StepTime);
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
                    this.Simulation.Dispose();
                    this.bufferPool.Clear();

                    foreach (var player in Player.GetWorldPlayers(this).ToList())
                        try
                        {
                            player.Destroy();
                        }
                        catch (Exception) { }

                    Heartbeat?.Dispose();
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
