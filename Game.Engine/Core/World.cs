namespace Game.Engine.Core
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using BepuPhysics.CollisionDetection;
    using BepuUtilities;
    using BepuUtilities.Collections;
    using BepuUtilities.Memory;
    using Game.API.Common;
    using Game.API.Common.Models;
    using Game.Engine.Core.Scoring;
    using Game.Engine.Core.SystemActors;
    using Game.Engine.Networking;
    using Game.Engine.Physics;
    using Nito.AsyncEx;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading;

    public class World : IDisposable
    {
        public readonly string WorldKey;
        public Random Random = new Random();
        public int AdvertisedPlayerCount { get; set; }
        public int SpectatorCount { get; set; }

        // the canonical game time, in milliseconds, from world start
        public uint PreviousTime { get; private set; } = 0;
        public uint Time { get; private set; } = 0;

        // offset between system clock and world start
        private readonly long OffsetTicks = 0;
        public uint LastTimingReport = 0;

        public Hook Hook { get; set; } = null;

        // lists of bodies, groups in the world
        readonly internal ConcurrentDictionary<BodyHandle, WorldBody> Bodies = new ConcurrentDictionary<BodyHandle, WorldBody>();
        public List<Group> Groups = new List<Group>();

        // list of IActors in the world. Actors are things that think.
        public List<IActor> Actors = new List<IActor>();

        public List<Player> Players = new List<Player>();
        public List<Connection> Connections = new List<Connection>();

        // timer for world step entry
        private Thread GameLoopThread = null;

        // most recent leaderboard available
        public Leaderboard Leaderboard = null;
        public bool ArenaRecordHasReset { get; set; } = false;
        public long ArenaRecordResetTime { get; set; } = 0;

        public Func<Fleet, Vector2> FleetSpawnPositionGenerator { get; set; }
        public Func<Leaderboard> LeaderboardGenerator { get; set; }
        public Func<Player, Fleet> NewFleetGenerator { get; set; }

        public GameConfiguration GameConfiguration { get; set; }

        public ScoringBase Scoring = new DefaultScoring();

        public readonly WorldMeshLoader MeshLoader;

        private uint LastObjectID = 1;

        public uint GenerateObjectID() { 
            return Interlocked.Increment(ref LastObjectID);
        }

        public bool CanSpawn { get => Hook.CanSpawn; set => Hook.CanSpawn = value; }
        public string CanSpawnReason { get; set; }
        public BufferPool BufferPool = new BufferPool();
        public Simulation Simulation = null;
        public CollidableProperty<WorldBodyProperties> BodyProperties;

        public SimpleThreadDispatcher ThreadDispatcher { get; }

        internal int ProjectileCount;

        public bool PendingDestruction;
        
        public World(Hook hook, GameConfiguration gameConfiguration, string worldKey)
        {
            this.GameConfiguration = gameConfiguration;
            this.WorldKey = worldKey;
            
            OffsetTicks = DateTime.Now.Ticks;
            Hook = hook ?? Hook.Default;

            Console.WriteLine($"Initializing World: {this.Hook.Name}");

            this.ThreadDispatcher = new SimpleThreadDispatcher(2);
            this.Simulation = Simulation.Create(
                BufferPool, 
                new NarrowPhaseCallbacks(this),
                new PoseIntegratorCallbacks(new Vector3(0, 0, 0)),
                
                //new SubsteppingTimestepper(10)
                //new PositionFirstTimestepper()
                new PositionLastTimestepper()
                
            );
            //this.Simulation.Statics.Add(new StaticDescription(new Vector3(0, -10, 0), new CollidableDescription(this.Simulation.Shapes.Add(new Box(100000, 10, 100000)), 0.1f)));
            NewFleetGenerator = this.DefaultNewFleetGenerator;

            this.MeshLoader = new WorldMeshLoader(this);

            InitializeSystemActors();

            GameLoopThread = new Thread((state) => WorldTickEntry());
            GameLoopThread.Start();
        }

        private Fleet DefaultNewFleetGenerator(Player player)
        {
            return new Fleet(this, player)
            {
                Owner = player,
                Caption = player.Name,
                Color = player.Color
            };
        }


        public bool InStep = false;
        // main entry to the world. This will be called every Hook.StepSize milliseconds, unless it's late. or early.
        public void Step()
        {
            try
            {
                var start = DateTime.Now;
                // calculate the new game time
                PreviousTime = Time;
                Time = (uint)((start.Ticks - OffsetTicks) / 10000);
                
                var dt = Time - PreviousTime;
                if (dt == 0)
                    dt = (uint)this.Hook.StepTime;
    

                foreach (var player in Player.GetWorldPlayers(this).ToList())
                    player.ControlCharacter();

                //Console.WriteLine("dt:" + dt);

                InStep = true;
                //this.Simulation.Timestep((float)Hook.StepTime, ThreadDispatcher);
                //Console.WriteLine($"Enter Step: {this.WorldKey}");
                this.Simulation.Timestep((float)Hook.StepTime);
                //Console.WriteLine($"Exit Step: {this.WorldKey}");
                InStep = false;

                foreach (var body in Bodies.Values)
                {
                    body.ReadSimulation();
                    body.IsInContact = false;
                }

                // execute collisions
                ref var bodyImpacts = ref ((NarrowPhase<NarrowPhaseCallbacks>)Simulation.NarrowPhase).Callbacks.BodyImpacts;
                if (true)
                {
                    for (int i = 0; i < bodyImpacts.Count; ++i)
                    {
                        ref var impact = ref bodyImpacts[i];

                        var aValid = Bodies.TryGetValue(impact.BodyHandleA, out WorldBody wbA);
                        var bValid = Bodies.TryGetValue(impact.BodyHandleB, out WorldBody wbB);

                        if (aValid)
                        {
                            if (wbA?.PendingDestruction == true || wbB?.PendingDestruction == true)
                                continue;

                            wbA.CollisionExecute(wbB);
                            wbA.IsInContact = true;

                            if (bValid)
                            {
                                wbB.CollisionExecute(wbA);
                                wbB.IsInContact = true;
                            }
                        }
                    }

                }
                bodyImpacts.Count = 0;

                ActorsThink();
                WriteSimulation();
                CheckTimings(start);
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception in server game loop:" + e.Message);
            }
            finally
            {
                InStep = false;
            }
        }

        private void InitializeSystemActors()
        {
            InitializeSystemActor<WorldResizer>();
            InitializeSystemActor<SpawnLocationsActor>();
            InitializeSystemActor<LeaderboardActor>();
            InitializeSystemActor<Authenticator>();
            InitializeSystemActor<Advertisement>();
            InitializeSystemActor<ObstacleTender>();
            InitializeSystemActor<AdvanceRetreat>();
            
            //InitializeSystemActor<CaptureTheFlag>();
            //InitializeSystemActor<Sumo>();
            //InitializeSystemActor<TeamColors>();
            //InitializeSystemActor<RoyaleMode>();
        }

        public T GetActor<T>()
        {
            return this.Actors.OfType<T>().FirstOrDefault();
        }

        private void InitializeSystemActor<T>()
            where T : class, IActor
        {
            Activator.CreateInstance(typeof(T), this);
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

                if (Time - LastTimingReport > 20000)
                {
                    LastTimingReport = Time;
                    Console.WriteLine($"{WorldKey} {elapsed}");
                }
            }
        }

        private void ActorsThink()
        {
            foreach (var actor in Actors.ToList())
                actor.Think();
        }

        private void WriteSimulation()
        {
            foreach (var body in Bodies.Values)
            {
                body.WriteSimulation();
            }
        }

        struct BroadPhaseOverlapEnumerator : IBreakableForEach<CollidableReference>
        {
            public QuickList<CollidableReference> References;
            //The enumerator never gets stored into unmanaged memory, so it's safe to include a reference type instance.
            public BufferPool Pool;
            public bool LoopBody(CollidableReference reference)
            {
                References.Allocate(Pool) = reference;

                //If you wanted to do any top-level filtering, this would be a good spot for it.
                //The CollidableReference tells you whether it's a body or a static object and the associated handle. You can look up metadata with that.
                return true;
            }
        }

        public IEnumerable<WorldBody> BodiesNear(Vector2 point, int maximumDistance)
        {
            if (InStep)
                throw new Exception("BodiesNear In step");

            var broadPhaseEnumerator = new BroadPhaseOverlapEnumerator {
                Pool = BufferPool,
                References = new QuickList<CollidableReference>(16, BufferPool)
            };

            Simulation.BroadPhase.GetOverlaps(
                new Vector3(point.X - maximumDistance / 2, -10, point.Y - maximumDistance / 2), 
                new Vector3(point.X + maximumDistance / 2, 10, point.Y + maximumDistance / 2), 
                ref broadPhaseEnumerator
            );

            var list = new WorldBody[broadPhaseEnumerator.References.Count];
            for (int overlapIndex = 0; overlapIndex < broadPhaseEnumerator.References.Count; ++overlapIndex)
            {
                var handle = broadPhaseEnumerator.References[overlapIndex].BodyHandle;
                var reference = Simulation.Bodies.GetBodyReference(handle);
                if (reference.Exists)
                {
                    if (Bodies.TryGetValue(handle, out var body))
                        list[overlapIndex] = body;
                }
                else
                {
                    var x = 1;
                }
            }

            broadPhaseEnumerator.References.Dispose(BufferPool);

            return list;
        }

        public void BodyAdd(WorldBody body)
        {
            if (InStep)
                throw new Exception("adding body during step");
            
            Bodies.AddOrUpdate(body.BodyHandle, body, (h, b) => {
                throw new Exception("Body already exists");
            });
        }

        public void BodyRemove(WorldBody body)
        {
            if (InStep)
                throw new Exception("removing body during step");
            Bodies.TryRemove(body.BodyHandle, out var trash);
        }

        public float DistanceOutOfBounds(Vector2 position, int buffer = 0)
        {
            var pos = Vector2.Abs(position);
            return Math.Max(pos.X - Hook.WorldSize + buffer, Math.Max(pos.Y - Hook.WorldSize + buffer, 0));
        }

        public Player CreatePlayer()
        {
            lock(Bodies)            
            {
                if (InStep)
                    throw new Exception("adding player instep");
                var player = new Player();
                player.Init(this);

                return player;
            }
        }

        private void WorldTickEntry()
        {
            try
            {
                var interval = TimeSpan.FromMilliseconds(Hook.StepTime);

                var nextTick = DateTime.Now + interval;
                while (!PendingDestruction)
                {
                    while ( DateTime.Now < nextTick )
                    {
                        var delay = nextTick - DateTime.Now;
                        if (delay.TotalMilliseconds > 0)
                            Thread.Sleep(delay);
                    }
                    nextTick += interval; // Notice we're adding onto when the last tick was supposed to be, not when it is now

                    lock(Bodies)
                    {
                        this.Step();
                        lock(Connections)
                            foreach (var connection in Connections)
                                connection.StepSyncInGameLoop();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in WorldTickEntry: " + e);
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
                    this.PendingDestruction = true;
                    this.Simulation.Dispose();
                    this.BufferPool.Clear();

                    foreach (var player in Player.GetWorldPlayers(this).ToList())
                        try
                        {
                            player.Destroy();
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
