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
    using Game.Engine.Core.SystemActors.CTF;
    using Game.Engine.Core.SystemActors.Royale;
    using Game.Engine.Networking;
    using Game.Engine.Physics;
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
        public uint PreviousTime { get; private set; } = 0;
        public uint Time { get; private set; } = 0;
        // offset between system clock and world start
        private readonly long OffsetTicks = 0;

        public Hook Hook { get; set; } = null;

        // lists of bodies, groups in the world
        public Dictionary<BodyHandle, WorldBody> Bodies = new Dictionary<BodyHandle, WorldBody>();
        public List<Group> Groups = new List<Group>();

        // list of IActors in the world. Actors are things that think.
        public List<IActor> Actors = new List<IActor>();

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

        private uint LastObjectID = 1;

        public uint GenerateObjectID() { 
            return Interlocked.Increment(ref LastObjectID);
        }

        public bool CanSpawn { get => Hook.CanSpawn; set => Hook.CanSpawn = value; }
        public string CanSpawnReason { get; set; }
        public BufferPool BufferPool = new BufferPool();
        public Simulation Simulation = null;
        public CollidableProperty<WorldBodyProperties> BodyProperties;
        internal int ProjectileCount;

        public bool PendingDestruction;
        
        public World(Hook hook, GameConfiguration gameConfiguration)
        {
            this.GameConfiguration = gameConfiguration;
            OffsetTicks = DateTime.Now.Ticks;
            Hook = hook ?? Hook.Default;
            GameID = Guid.NewGuid().ToString().Replace("-", "");

            Console.WriteLine($"Initializing World: {this.Hook.Name}");

            BodyProperties = new CollidableProperty<WorldBodyProperties>();

            this.Simulation = Simulation.Create(
                BufferPool, 
                new NarrowPhaseCallbacks(this) 
                {
                    Properties = BodyProperties
                },
                new PoseIntegratorCallbacks(new Vector3(0, 0, 0)),
                
                
                //new SubsteppingTimestepper(10)
                //new PositionFirstTimestepper()
                new PositionLastTimestepper()
            );
            //this.Simulation.Statics.Add(new StaticDescription(new Vector3(0, -10, 0), new CollidableDescription(this.Simulation.Shapes.Add(new Box(100000, 10, 100000)), 0.1f)));
            NewFleetGenerator = this.DefaultNewFleetGenerator;


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

        // main entry to the world. This will be called every Hook.StepSize milliseconds, unless it's late. or early.
        public void Step()
        {
            var start = DateTime.Now;
            // calculate the new game time
            PreviousTime = Time;
            Time = (uint)((start.Ticks - OffsetTicks) / 10000);
            
            var dt = Time - PreviousTime;
            if (dt == 0)
                dt = (uint)this.Hook.StepTime;

            // this is a bad way to do this... just overestimating potentential number of contacts... though what's the 4-color theorem equivalent for spheres?
            ref var bodyImpacts = ref ((NarrowPhase<NarrowPhaseCallbacks>)Simulation.NarrowPhase).Callbacks.BodyImpacts;
            bodyImpacts.EnsureCapacity(this.Bodies.Count * 10, this.BufferPool);

            foreach (var player in Player.GetWorldPlayers(this).ToList())
                player.ControlCharacter();

            this.Simulation.Timestep(dt);
            // Ensure that every body as a new handle... else .. <cringe>
            UpdateBodyReferences();

            // execute collisions
            for (int i = 0; i < bodyImpacts.Count; ++i)
            {
                ref var impact = ref bodyImpacts[i];

                var aValid = Bodies.TryGetValue(impact.BodyHandleA, out WorldBody wbA);
                var bValid = Bodies.TryGetValue(impact.BodyHandleB, out WorldBody wbB);

                /*if (impact.CustomBounce)
                    wbA.LinearVelocity = new Vector2(impact.newAVelocity.X, impact.newAVelocity.Z);*/

                if (aValid)
                {
                    wbA.CollisionExecute(wbB);
                    wbA.IsInContact = true;
                    if (bValid)
                    {
                        wbB.CollisionExecute(wbA);
                        wbB.IsInContact = true;
                    }
                }
            }
            bodyImpacts.Count = 0;

            ActorsThink();
            CheckTimings(start);
        }

        private void InitializeSystemActors()
        {
            //InitializeSystemActor<WorldResizer>();
            InitializeSystemActor<WorldMeshLoader>();
            InitializeSystemActor<SpawnLocationsActor>();
            InitializeSystemActor<LeaderboardActor>();
            InitializeSystemActor<Authenticator>();
            InitializeSystemActor<Advertisement>();
            InitializeSystemActor<ObstacleTender>();
            InitializeSystemActor<CaptureTheFlag>();
            InitializeSystemActor<Sumo>();
            InitializeSystemActor<TeamColors>();
            InitializeSystemActor<RoomReset>();
            InitializeSystemActor<RoyaleMode>();
            InitializeSystemActor<AdvanceRetreat>();
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
            }
        }

        private void ActorsThink()
        {
            foreach (var actor in Actors.ToList())
                actor.Think();
        }

        private void UpdateBodyReferences()
        {
            foreach (var body in Bodies.Values)
            {
                var reference = Simulation.Bodies.GetBodyReference(body.BodyHandle);
                body.UpdateBodyReference(reference);
                body.IsInContact = false;
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
                list[overlapIndex] = Bodies[broadPhaseEnumerator.References[overlapIndex].BodyHandle];

            broadPhaseEnumerator.References.Dispose(BufferPool);

            return list;
        }

        public void BodyAdd(WorldBody body)
        {
            Bodies.Add(body.BodyHandle, body);
        }

        public void BodyRemove(WorldBody body)
        {
            Bodies.Remove(body.BodyHandle);
        }

        public float DistanceOutOfBounds(Vector2 position, int buffer = 0)
        {
            var pos = Vector2.Abs(position);
            return Math.Max(pos.X - Hook.WorldSize + buffer, Math.Max(pos.Y - Hook.WorldSize + buffer, 0));
        }

        private void WorldTickEntry()
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

                this.Step();
                ConnectionHeartbeat.Step();
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
