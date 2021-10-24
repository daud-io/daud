using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities.Collections;
using Game.Engine.Core;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Game.Engine.Physics
{
    unsafe struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public QuickList<BodyImpacts> BodyImpacts;
        public World World;

        public NarrowPhaseCallbacks(World world) : this()
        {
            this.World = world;
            this.BodyImpacts = new QuickList<BodyImpacts>();
        }

        public void Initialize(Simulation simulation)
        {
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
                try
                {
                    var worldBodyA = World.Bodies[a.BodyHandle];
                    var worldBodyB = World.Bodies[b.BodyHandle];

                    var responseAB = worldBodyA.CanCollide(worldBodyB);
                    var responseBA = worldBodyB.CanCollide(worldBodyA);

                    return responseAB.CanCollide || responseBA.CanCollide;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
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
        void AddBodyImpact(BodyHandle A, CollidableReference impactedCollidable, Vector3 newAVelocity, bool customBounce)
        {
            ref var newImpact = ref BodyImpacts.Allocate(World.BufferPool);
            newImpact.BodyHandleA = A;
            if (impactedCollidable.Mobility != CollidableMobility.Static)
                newImpact.BodyHandleB = impactedCollidable.BodyHandle;
            else
                newImpact.BodyHandleB = new BodyHandle(-1);

            newImpact.CustomBounce = customBounce;
            newImpact.newAVelocity = newAVelocity;
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

            pairMaterial.FrictionCoefficient = 0;
            pairMaterial.MaximumRecoveryVelocity = 4f;
            pairMaterial.SpringSettings = new SpringSettings(13, 0);

            try
            {
                var worldBodyA = World.Bodies[pair.A.BodyHandle];
                var worldBodyB = World.Bodies[pair.B.BodyHandle];

                var responseAB = worldBodyA.CanCollide(worldBodyB);
                var responseBA = worldBodyB.CanCollide(worldBodyA);

                if (responseAB.CanCollide)
                    pairMaterial.FrictionCoefficient = MathF.Max(responseAB.FrictionCoefficient, pairMaterial.FrictionCoefficient);
                if (responseBA.CanCollide)
                    pairMaterial.FrictionCoefficient = MathF.Max(responseBA.FrictionCoefficient, pairMaterial.FrictionCoefficient);

                for (int i = 0; i < manifold.Count; ++i)
                    if (manifold.GetDepth(ref manifold, i) >= -1e-3f)
                    {
                        AddBodyImpact(pair.A.BodyHandle, pair.B, default, false);

                        if (true
                            && (responseAB.HasImpact || responseBA.HasImpact)
                            && (pair.B.Mobility == CollidableMobility.Static && manifold.Count == 1))
                        {
                            // simulate a "satisfying" bounce
                            // this is not physically accurate

                            // not using body's linearvelocity because it was speculatively slowed
                            //var reference = World.Simulation.Bodies.GetBodyReference(pair.A.BodyHandle);
                            //Vector3 incoming = reference.Velocity.Linear;

                            if (!worldBodyA.IsBouncing)
                            {
                                //instead use a moving average of the linear velocity over the last few frames
                                Vector3 incoming = new Vector3(worldBodyA.AverageLinearVelocity.X, 0, worldBodyA.AverageLinearVelocity.Y);
                                float incomingLength = incoming.Length();
                                if (incomingLength > 1.0f)
                                {
                                    Vector3 normal = manifold.GetNormal(ref manifold, 0);
                                    Vector3 reflection = (incoming - 2 * Vector3.Dot(incoming, normal) * normal) * 0.66f;

                                    var angle = Vector3.Dot(reflection, normal) / reflection.Length() / normal.Length();
                                    if (angle > 0.3)
                                    {

                                        reflection = Vector3.Normalize(reflection) * incomingLength;
                                        worldBodyA.LinearVelocity = new Vector2(reflection.X, reflection.Z);
                                        worldBodyA.WriteSimulation();
                                        worldBodyA.IsBouncing = true;

                                        return false;
                                    }
                                }
                            }
                            if (worldBodyA.IsBouncing)
                                return false;
                        }

                        break;
                    }


                if (responseAB.HasImpact || responseBA.HasImpact)
                    return true; // bouncy
                else
                    return false; // no bouncy
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return false;
            }
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
