namespace Game.Engine.Core
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using BepuPhysics.CollisionDetection;
    using BepuUtilities.Memory;
    using Game.Engine.Networking;
    using RBush;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class World : IDisposable
    {
        public uint Time { get; private set; } = 0;
        public uint LastStepSize { get; private set; } = 0;
        private readonly long OffsetTicks = 0;

        public Hook Hook { get; set; } = null;

        private Timer Heartbeat = null;

        private readonly List<IDisposable> Disposables = new List<IDisposable>();

        private RBush<Body> RTree = new RBush<Body>();
        public List<Body> Bodies = new List<Body>();
        public List<Group> Groups = new List<Group>();
        public List<IActor> Actors = new List<IActor>();

        private readonly BufferPool BufferPool = new BufferPool();
        public Simulation Simulation = null;

        private long TimeLeaderboardRecalc = 0;
        public Leaderboard Leaderboard = null;

        private bool Processing = false;

        public Func<Fleet, Vector2> FleetSpawnPositionGenerator { get; set; }
        public Func<Leaderboard> LeaderboardGenerator { get; set; }
        public Func<Player, string, Fleet> NewFleetGenerator { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }

        public string[] AllowedColors { get; set; }

        public int AdvertisedPlayerCount { get; set; }

        public string Image { get; set; } = "default";

        BodyDescription bulletDescription;

        public World()
        {
            Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0,0,0)));

            var bulletShape = new Sphere(0.5f);
            bulletShape.ComputeInertia(.1f, out var bulletInertia);
            bulletDescription = BodyDescription.CreateDynamic(new Vector3(), bulletInertia, new CollidableDescription(Simulation.Shapes.Add(bulletShape), 10), new BodyActivityDescription(0.01f));

            bulletDescription.Pose.Position = new Vector3(0, 0, 0);
            bulletDescription.Velocity.Linear = new Vector3(0.1f, 0.1f, 0);
            Simulation.Bodies.Add(bulletDescription);

            OffsetTicks = DateTime.Now.Ticks;
            Hook = Hook.Default;

            SystemActor<Advertisement>();
            SystemActor<RobotTender>();
            SystemActor<ObstacleTender>();
            SystemActor<CaptureTheFlag>();

            InitializeStepTimer();
        }

        private void SystemActor<T>()
            where T : class, IActor, new()
        {
            var actor = new T();
            actor.Init(this);
        }


        void AddBodyShape(Shapes shapes, Bodies bodies, int setIndex, int indexInSet)
        {
            ref var set = ref bodies.Sets[setIndex];
            Vector3 color = new Vector3(1, 1, 1);

            AddShape(shapes, set.Collidables[indexInSet].Shape, ref set.Poses[indexInSet], color);
        }

        public unsafe void AddShape(Shapes shapes, TypedIndex shapeIndex, ref RigidPose pose, in Vector3 color)
        {
            if (shapeIndex.Exists)
            {
                shapes[shapeIndex.Type].GetShapeData(shapeIndex.Index, out var shapeData, out _);
                AddShape(shapeData, shapeIndex.Type, shapes, ref pose, color);
            }
        }
        public unsafe void AddShape(void* shapeData, int shapeType, Shapes shapes, ref RigidPose pose, in Vector3 color)
        {
            switch (shapeType)
            {
                case Sphere.Id:
                    {
                        var position = pose.Position;
                        var sphere = Unsafe.AsRef<Sphere>(shapeData);
                        Console.WriteLine(pose.Position.ToString());
                    }
                    break;
            }
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

                Simulation.Timestep(LastStepSize / 1000f);

                for (int i = 0; i < Simulation.Bodies.Sets.Length; ++i)
                {
                    ref var set = ref Simulation.Bodies.Sets[i];
                    if (set.Allocated) //Islands are stored noncontiguously; skip those which have been deallocated.
                    {
                        for (int bodyIndex = 0; bodyIndex < set.Count; ++bodyIndex)
                        {
                            AddBodyShape(Simulation.Shapes, Simulation.Bodies, i, bodyIndex);
                        }
                    }
                }
                for (int i = 0; i < Simulation.Statics.Count; ++i)
                {
                    //AddStaticShape(Simulation.Shapes, Simulation.Statics, i);
                }




                RTree.Clear();
                foreach (var body in Bodies)
                    body.Project(Time);

                RTree.BulkLoad(Bodies);

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
                    if (body.IsDirty)
                    {
                        body.DefinitionTime = this.Time;
                        body.OriginalPosition = body.Position;
                        body.OriginalAngle = body.Angle;
                        body.IsDirty = false;
                    }

                ProcessLeaderboard();

                var elapsed = DateTime.Now.Subtract(start).TotalMilliseconds;
                if (elapsed > Hook.StepTime)
                    Console.WriteLine($"**** 100% processing time warning: {elapsed}");
                else if (elapsed > Hook.StepTime * 0.8f)
                    Console.WriteLine($"*** 80% processing time warning: {elapsed}");
                else if (elapsed > Hook.StepTime * 0.5f)
                    Console.WriteLine($"** 50% processing time warning: {elapsed}");
            }
            Processing = false;
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
            return _id++;
        }

        public IEnumerable<Body> BodiesNear(Vector2 point, int maximumDistance = 0, bool offsetSize = false)
        {
            if (maximumDistance == 0)
                return this.Bodies;
            else
                return RTree.Search(new Envelope(
                    point.X - maximumDistance / 2,
                    point.Y - maximumDistance / 2,
                    point.X + maximumDistance / 2,
                    point.Y + maximumDistance / 2
                ));
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
