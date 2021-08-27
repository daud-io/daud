namespace Game.Engine.Core
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using Game.API.Common;
    using Game.Engine.Physics;
    using System;
    using System.Numerics;

    public class WorldBody : IActor
    {
        public struct CollisionResponse
        {
            public CollisionResponse(bool canCollide = false, bool hasImpact = false)
            {
                this.CanCollide = canCollide;
                this.HasImpact = hasImpact;

            }

            public bool CanCollide;
            public bool HasImpact;
        }

        public readonly World World;
        public uint ID;

        protected TypedIndex ShapeHandle;

        internal BodyHandle BodyHandle;

        public Group Group;

        public bool Exists;
        public long SleepUntil;

        public byte Mode = 0;
        public Sprites Sprite;
        public string Color;
        protected int CycleMS = 0;
        public bool PendingDestruction = false;
        public bool ContactLastFrame = false;

        public WorldBody(World world)
        {
            this.World = world;
            if (world.InStep)
                throw new Exception("Tried to create WorldBody while processing step");

            this.BodyHandle.Value = -1;

            this.ID = World.GenerateObjectID();
            this.Mass = 100;
            this.Size = 100;

            this.DefinePhysicsObject(this.Size, this.Mass);
            this.WriteSimulation();
            
            World.BodyAdd(this);
            World.Actors.Add(this);
            this.Exists = true;
        }

        protected virtual void DefinePhysicsObject(float size, float mass)
        {
            var shape = new Sphere(size);
            var position2d = InitialPosition();

            ShapeHandle = World.Simulation.Shapes.Add(shape);
            BodyHandle = World.Simulation.Bodies.Add(BodyDescription.CreateDynamic(
                new Vector3(position2d.X, 0, position2d.Y),
                GetBodyInertia(shape, mass),
                new CollidableDescription(ShapeHandle, 150f),
                //new CollidableDescription(ShapeHandle, 0.1f, ContinuousDetectionSettings.Continuous(1e-4f, 1e-4f)),
                new BodyActivityDescription(0.01f)
            ));

            ref var worldBodyProperties = ref World.BodyProperties.Allocate(BodyHandle);
            worldBodyProperties = new WorldBodyProperties
            {
                Friction = 0.1f
            };
        }
        
        protected virtual BodyInertia GetBodyInertia(IConvexShape shape, float mass)
        {
            shape.ComputeInertia(mass, out var bodyInertia);
            
            // this locks rotation along the X and Z axes, aiding 2d physics
            bodyInertia.InverseInertiaTensor.XX = 0;
            bodyInertia.InverseInertiaTensor.ZZ = 0;

            return bodyInertia;
        }

        protected virtual Vector2 InitialPosition()
        {
            return Vector2.Zero;
        }

        public Vector2 Position;
        public Vector2 LinearVelocity;

        public float AngularVelocity;
        public float Angle;

        protected int size = 100;
        protected bool sizeDirty;
        public int Size
        {
            get => size;
            set
            {
                if (value != size)
                    sizeDirty = true;

                size = value;
            }
        }

        private float mass;
        private bool massDirty;
        public float Mass {
            get => mass;
            set
            {
                if (mass != value)
                    this.massDirty = true; 

                mass = value;
            }
        }

        public Vector2 AverageLinearVelocity;
        public bool IsBouncing;
        public bool IsInContact;

        protected void BecomeKinematic()
        {
            World.Simulation.Bodies
                .GetBodyReference(this.BodyHandle)
                .BecomeKinematic();
        }

        public void ReadSimulation()
        {
            var body = World.Simulation.Bodies.GetBodyReference(this.BodyHandle);

            Position = Vector3ToVector2(body.Pose.Position);
            LinearVelocity = Vector3ToVector2(body.Velocity.Linear);;

            AngularVelocity = body.Velocity.Angular.Y;
            Angle = ToEulerAngles(body.Pose.Orientation).Y;
        }

        public void WriteSimulation()
        {
            if (this.BodyHandle.Value == -1)
                throw new Exception("Cannot write simulation with invalid body handle");

            var body = World.Simulation.Bodies.GetBodyReference(this.BodyHandle);

            body.Pose.Position = new Vector3(Position.X, 0, Position.Y);
            body.Velocity.Linear = new Vector3(LinearVelocity.X, 0, LinearVelocity.Y);

            body.Velocity.Angular.Y = AngularVelocity;
            body.Pose.Orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), Angle);

            if (massDirty || sizeDirty)
            {
                ref var shape = ref World.Simulation.Shapes.GetShape<Sphere>(ShapeHandle.Index);
                if (massDirty)
                    body.LocalInertia = GetBodyInertia(shape, Mass);
                if (massDirty)
                    shape.Radius = size;

                massDirty = sizeDirty = false;
                
            }

            body.Awake = true;
            body.Pose.Position.Y = 0;
        }

        private Vector3 ToEulerAngles(Quaternion q)
        {
            // Store the Euler angles in radians
            Vector3 pitchYawRoll = new Vector3();

            double sqw = q.W * q.W;
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            double unit = sqx + sqy + sqz + sqw;
            double test = q.X * q.Y + q.Z * q.W;

            if (test > 0.4999f * unit)                              // 0.4999f OR 0.5f - EPSILON
            {
                // Singularity at north pole
                pitchYawRoll.Y = 2f * (float)Math.Atan2(q.X, q.W);  // Yaw
                pitchYawRoll.X = MathF.PI * 0.5f;                         // Pitch
                pitchYawRoll.Z = 0f;                                // Roll
                return pitchYawRoll;
            }
            else if (test < -0.4999f * unit)                        // -0.4999f OR -0.5f + EPSILON
            {
                // Singularity at south pole
                pitchYawRoll.Y = -2f * (float)Math.Atan2(q.X, q.W); // Yaw
                pitchYawRoll.X = -MathF.PI * 0.5f;                        // Pitch
                pitchYawRoll.Z = 0f;                                // Roll
                return pitchYawRoll;
            }
            else
            {
                pitchYawRoll.Y = (float)Math.Atan2(2f * q.Y * q.W - 2f * q.X * q.Z, sqx - sqy - sqz + sqw);       // Yaw
                pitchYawRoll.X = (float)Math.Asin(2f * test / unit);                                             // Pitch
                pitchYawRoll.Z = (float)Math.Atan2(2f * q.X * q.W - 2f * q.Y * q.Z, -sqx + sqy - sqz + sqw);      // Roll
            }

            return pitchYawRoll;
        }

        private Vector2 Vector3ToVector2(Vector3 vector3)
        {
            return new Vector2
            {
                X = vector3.X,
                Y = vector3.Z
            };
        }

        protected virtual void Update()
        {
            this.AverageLinearVelocity = this.AverageLinearVelocity * 0.75f + LinearVelocity * 0.25f;
            if (!this.IsInContact)
                this.IsBouncing = false;
        }

        void IActor.Think()
        {
            if (Exists)
            {
                if (PendingDestruction)
                    Destroy();
                else
                {
                    if (World != null && World.Time > SleepUntil)
                    {
                        this.Update();

                        if (World != null)
                            SleepUntil = World.Time + CycleMS;
                    }

                    if (PendingDestruction)
                        Destroy();
                }
            }
        }

        protected virtual void Die()
        {
            this.PendingDestruction = true;
        }


        public virtual void Destroy()
        {
            World.Actors.Remove(this);
            World.BodyRemove(this);
            World.Simulation.Bodies.Remove(this.BodyHandle);
            World.Simulation.Shapes.Remove(this.ShapeHandle);
            this.BodyHandle.Value = -1;
            this.ShapeHandle.Value = -1;
            this.Exists = false;
        }

        public virtual CollisionResponse CanCollide(WorldBody otherBody)
        {
            return new CollisionResponse
            {
                CanCollide = false,
                HasImpact = false
            };
        }

        public virtual void CollisionExecute(WorldBody projectedBody)
        {
            
        }
    }
}
