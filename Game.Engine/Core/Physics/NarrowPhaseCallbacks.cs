using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities.Collections;
using Game.Engine.Core;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Game.Engine.Physics
{
    unsafe struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public CollidableProperty<WorldBodyProperties> Properties;
        public SpinLock ProjectileLock;
        public QuickList<BodyImpacts> BodyImpacts;
        public World World;
        
        public NarrowPhaseCallbacks(World world): this()
        {
            this.World = world;
        }

        public void Initialize(Simulation simulation)
        {
            Properties.Initialize(simulation);
        }

        /// <summary>
        /// Chooses whether to allow contact generation to proceed for two overlapping collidables.
        /// </summary>
        /// <param name="workerIndex">Index of the worker that identified the overlap.</param>
        /// <param name="a">Reference to the first collidable in the pair.</param>
        /// <param name="b">Reference to the second collidable in the pair.</param>
        /// <returns>True if collision detection should proceed, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
        {
            //It's impossible for two statics to collide, and pairs are sorted such that bodies always come before statics.
            if (b.Mobility != CollidableMobility.Static)
            {
                var worldBodyA = World.Bodies[a.BodyHandle];
                var worldBodyB = World.Bodies[b.BodyHandle];
                
                bool isCollision = false;

                isCollision = worldBodyA.IsCollision(worldBodyB)
                    || worldBodyB.IsCollision(worldBodyA);

                return isCollision;
            }
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        /// <summary>
        /// Chooses whether to allow contact generation to proceed for the children of two overlapping collidables in a compound-including pair.
        /// </summary>
        /// <param name="pair">Parent pair of the two child collidables.</param>
        /// <param name="childIndexA">Index of the child of collidable A in the pair. If collidable A is not compound, then this is always 0.</param>
        /// <param name="childIndexB">Index of the child of collidable B in the pair. If collidable B is not compound, then this is always 0.</param>
        /// <returns>True if collision detection should proceed, false otherwise.</returns>
        /// <remarks>This is called for each sub-overlap in a collidable pair involving compound collidables. If neither collidable in a pair is compound, this will not be called.
        /// For compound-including pairs, if the earlier call to AllowContactGeneration returns false for owning pair, this will not be called. Note that it is possible
        /// for this function to be called twice for the same subpair if the pair has continuous collision detection enabled; 
        /// the CCD sweep test that runs before the contact generation test also asks before performing child pair tests.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            //This is similar to the top level broad phase callback above. It's called by the narrow phase before generating
            //subpairs between children in parent shapes. 
            //This only gets called in pairs that involve at least one shape type that can contain multiple children, like a Compound.
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddBodyImpact(BodyHandle A, CollidableReference impactedCollidable)
        {
            bool lockTaken = false;
            ProjectileLock.Enter(ref lockTaken);
            try
            {
                ref var newImpact = ref BodyImpacts.AllocateUnsafely();
                newImpact.BodyHandleA = A;
                if (impactedCollidable.Mobility != CollidableMobility.Static)
                {
                    ref var properties = ref Properties[impactedCollidable.BodyHandle];
                    newImpact.BodyHandleB = impactedCollidable.BodyHandle;
                }
                else
                    newImpact.BodyHandleB = new BodyHandle(-1);
            }
            finally
            {
                if (lockTaken)
                    ProjectileLock.Exit();
            }
        }        

        /// <summary>
        /// Provides a notification that a manifold has been created for a pair. Offers an opportunity to change the manifold's details. 
        /// </summary>
        /// <param name="workerIndex">Index of the worker thread that created this manifold.</param>
        /// <param name="pair">Pair of collidables that the manifold was detected between.</param>
        /// <param name="manifold">Set of contacts detected between the collidables.</param>
        /// <param name="pairMaterial">Material properties of the manifold.</param>
        /// <returns>True if a constraint should be created for the manifold, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
        {
            ref var propertiesA = ref Properties[pair.A.BodyHandle];
            pairMaterial.FrictionCoefficient = propertiesA.Friction;
            if (pair.B.Mobility != CollidableMobility.Static)
            {
                //If two bodies collide, just average the friction. Other options include min(a, b) or a * b.
                ref var propertiesB = ref Properties[pair.B.BodyHandle];
                pairMaterial.FrictionCoefficient = (pairMaterial.FrictionCoefficient + propertiesB.Friction) * 0.5f;
            }

            pairMaterial.MaximumRecoveryVelocity = float.MaxValue;
            pairMaterial.SpringSettings = new SpringSettings(30, 0);

            for (int i = 0; i < manifold.Count; ++i)
            {
                if (manifold.GetDepth(ref manifold, i) >= -1e-3f)
                {
                    //An actual collision was found. 
                    AddBodyImpact(pair.A.BodyHandle, pair.B);
                    break;
                }
            }
            return true;
        }

        /// <summary>
        /// Provides a notification that a manifold has been created between the children of two collidables in a compound-including pair.
        /// Offers an opportunity to change the manifold's details. 
        /// </summary>
        /// <param name="workerIndex">Index of the worker thread that created this manifold.</param>
        /// <param name="pair">Pair of collidables that the manifold was detected between.</param>
        /// <param name="childIndexA">Index of the child of collidable A in the pair. If collidable A is not compound, then this is always 0.</param>
        /// <param name="childIndexB">Index of the child of collidable B in the pair. If collidable B is not compound, then this is always 0.</param>
        /// <param name="manifold">Set of contacts detected between the collidables.</param>
        /// <returns>True if this manifold should be considered for constraint generation, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            // in daud, so far we have no compound objects
            return true;
        }

        /// <summary>
        /// Releases any resources held by the callbacks. Called by the owning narrow phase when it is being disposed.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
