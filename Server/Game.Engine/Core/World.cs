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
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Threading;

    public class World : IDisposable
    {
        public readonly string WorldKey;
        public Random Random = new Random();
        public Dictionary<string, List<Vector2>> SpawnPoints = new Dictionary<string, List<Vector2>>();
        public int AdvertisedPlayerCount { get; set; }
        public int SpectatorCount { get; set; }

        // the canonical game time, in milliseconds, from world start
        public float FloatTime = 0;
        public uint Time { get; private set; } = 0;

        // offset between system clock and world start
        private readonly long OffsetTicks = 0;
        public uint LastTimingReport = 0;

        public Hook Hook { get; set; } = null;
        public int HookHash { get; private set; } = 0;

        // lists of bodies, groups in the world
        internal readonly Dictionary<BodyHandle, WorldBody> Bodies = new Dictionary<BodyHandle, WorldBody>();
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
        public void Step(float dt)
        {
            long tStart, tControl, tActors, tWrite, tPhysics, tRead, tCollisions, tCleanup, tHash;
            tStart = Stopwatch.GetTimestamp();
            try
            {
                FloatTime += dt;
                Time = (uint)FloatTime;

                //Console.WriteLine($"dt:{dt} time: {Time}");
                
                foreach (var player in Player.GetWorldPlayers(this))
                    player.ControlCharacter();

                tControl = Stopwatch.GetTimestamp();

                ActorsThink(dt);

                tActors = Stopwatch.GetTimestamp();
                WriteSimulation();
                tWrite = Stopwatch.GetTimestamp();

                InStep = true;
                //this.Simulation.Timestep(dt, ThreadDispatcher);
                this.Simulation.Timestep(dt);
                InStep = false;
                tPhysics = Stopwatch.GetTimestamp();

                foreach (var body in Bodies.Values)
                {
                    body.ReadSimulation();
                    body.IsInContact = false;
                }
                tRead = Stopwatch.GetTimestamp();

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

                tCollisions = Stopwatch.GetTimestamp();

                ActorsCleanup();
                tCleanup = Stopwatch.GetTimestamp();
                this.HookHash = this.Hook.GetHashCode();

                tHash = Stopwatch.GetTimestamp();

                StepTimings(tStart, tControl-tStart, tActors-tStart, tWrite-tStart, tPhysics-tStart, tRead-tStart, tCollisions-tStart, tCleanup-tStart, tHash-tStart);
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception in server game loop:" + e);
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
            InitializeSystemActor<TeamColors>();
            
            //InitializeSystemActor<CaptureTheFlag>();
            //InitializeSystemActor<Sumo>();
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

        private void StepTimings(long tStart, long tControl, long tActors, long tWrite, long tPhysics, long tRead, long tCollisions, long tCleanup, long tHash)
        {
            float scale = 1_000_000;
            //Console.WriteLine($"control:{tControl/scale:0.0}\tactors:{tActors/scale:0.0}\twrite:{tWrite/scale:0.0}\tphysics:{tPhysics/scale:0.0}\tread:{tRead/scale:0.0}\tcollisions:{tCollisions/scale:0.0}\tcleanup:{tCleanup/scale:0.0}\thash:{tHash/scale:0.0}");
        }

        private void CheckTimings(long tStart, long tLock, long tSteps, long tNet)
        {
            float scale = 1_000_000;
            //Console.WriteLine($"lock:{tLock/scale:0.0}\tsteps:{tSteps/scale:0.0}\t net:{tNet/scale:0.0}");
            
            if (Time - LastTimingReport > 10000)
            {
                LastTimingReport = Time;

                /*ThreadPool.GetAvailableThreads(out int threadsAvailable, out int iocpAvailable);
                ThreadPool.GetMinThreads(out int threadsMin, out int iocpMin);
                
                Console.WriteLine($"{WorldKey} {elapsed} threads:{ThreadPool.ThreadCount} {threadsAvailable}/{threadsMin} iocp:{iocpAvailable}/{iocpMin} iocp-pending:{ThreadPool.PendingWorkItemCount}");*/
            }

            /*if (Time > 10000) // lets not get too excited if things slow down when initialized
            {
                if (elapsed > Hook.StepTime)
                    Console.WriteLine($"**** 100% processing time warning: {elapsed}");
                else if (elapsed > Hook.StepTime * 0.8f)
                    Console.WriteLine($"*** 80% processing time warning: {elapsed}");
                else if (elapsed > Hook.StepTime * 0.5f)
                    Console.WriteLine($"** 50% processing time warning: {elapsed}");

            }*/


            
        }

        private void ActorsThink(float dt)
        {
            foreach (var actor in Actors.ToList())
                actor.Think(dt);
        }

        private void ActorsCleanup()
        {
            foreach (var actor in Actors.ToList())
                actor.Cleanup();
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
            var bodies = new List<WorldBody>();
            this.BodiesNear(point, maximumDistance, (body) => bodies.Add(body));
            return bodies;  
        }

        public void BodiesNear(Vector2 point, int maximumDistance, Action<WorldBody> action)
        {
            if (InStep)
                throw new Exception("BodiesNear In step");

            var broadPhaseEnumerator = new BroadPhaseOverlapEnumerator
            {
                Pool = BufferPool,
                References = new QuickList<CollidableReference>(64, BufferPool)
            };

            Simulation.BroadPhase.GetOverlaps(
                new Vector3(point.X - maximumDistance / 2, -10, point.Y - maximumDistance / 2),
                new Vector3(point.X + maximumDistance / 2, 10, point.Y + maximumDistance / 2),
                ref broadPhaseEnumerator
            );

            for (int overlapIndex = 0; overlapIndex < broadPhaseEnumerator.References.Count; ++overlapIndex)
            {
                var handle = broadPhaseEnumerator.References[overlapIndex].BodyHandle;
                var reference = Simulation.Bodies.GetBodyReference(handle);
                if (reference.Exists)
                    if (Bodies.TryGetValue(handle, out var body))
                        action(body);
            }

            broadPhaseEnumerator.References.Dispose(BufferPool);
        }

        public void BodyAdd(WorldBody body)
        {
            if (InStep)
                throw new Exception("adding body during step");
            
            Bodies.Add(body.BodyHandle, body);
        }

        public void BodyRemove(WorldBody body)
        {
            if (InStep)
                throw new Exception("removing body during step");
            Bodies.Remove(body.BodyHandle, out var trash);
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
            var sw = new Stopwatch();

            try
            {
                var lastRun = DateTime.Now;
                var dt = 0d;

                while (!PendingDestruction)
                {
                    
                    long tStart, tLock, tSteps, tNet;
                    tStart = Stopwatch.GetTimestamp();
                    lock(Bodies)
                    {
                        tLock = Stopwatch.GetTimestamp();

                        var now = DateTime.Now;
                        dt += now.Subtract(lastRun).TotalMilliseconds;
                        lastRun = now;

                        int steps = 0;
                        while (dt > 3)
                        {
                            steps++;

                            float thisStep = MathF.Min(Hook.StepTime, (float)dt);
                            dt -= thisStep;
                            this.Step(thisStep);
                        }
                        tSteps = Stopwatch.GetTimestamp();

                        if (steps > 0)
                        {
                            if (steps > 2)
                                Console.WriteLine($"flushing {steps}");

                            var networkstart = DateTime.Now;
                            lock (Connections)
                                foreach (var connection in Connections)
                                    connection.StepSyncInGameLoop();

                            if (DateTime.Now.Subtract(networkstart).TotalMilliseconds > 5)
                                Console.WriteLine($"slow network flush: {DateTime.Now.Subtract(networkstart).TotalMilliseconds}");
                        }
                        tNet = Stopwatch.GetTimestamp();
                    }

                    // calculate when the next server frame should be
                    var nextTick = lastRun.AddMilliseconds(Hook.StepTime);
                    var delta = nextTick - DateTime.Now;
                    if (delta.TotalMilliseconds < 0)
                        Console.WriteLine($"Late Tick: {delta.TotalMilliseconds}");

                    if (delta.TotalMilliseconds > 1)
                    {
                        sw.Restart();
                        Thread.Sleep(delta);
                        sw.Stop();
                        if (Math.Abs((sw.Elapsed - delta).TotalMilliseconds) > 5)
                            Console.WriteLine($"sleep {delta.TotalMilliseconds}: " + (sw.Elapsed.TotalMilliseconds));
                    }


                    CheckTimings(tStart, tLock-tStart, tSteps-tStart, tNet-tStart);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in WorldTickEntry: " + e);
            }
        }

        public Vector2 ChooseSpawnPoint(string type, object obj)
        {
            var alias = type;

            if (SpawnPoints.TryGetValue(alias, out var locations))
                return locations[Random.Next(0, locations.Count)];

            if (type == "fleet" && obj is Fleet fleet)
                if (FleetSpawnPositionGenerator != null)
                    return FleetSpawnPositionGenerator(fleet);

            return RandomPosition();
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

