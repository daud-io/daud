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
        public readonly World World;
        public uint ID;

        protected TypedIndex ShapeHandle;

        internal BodyHandle BodyHandle;

        public Group Group;

        public bool Exists;

        public byte Mode = 0;
        public Sprites Sprite;
        public string Color;

        protected BodyReference BodyReference;
        public bool PendingDestruction = false;

        public WorldBody(World world)
        {
            this.World = world;

            this.ID = World.GenerateObjectID();
            
            this.DefinePhysicsObject(this.Size, this.Mass);

            this.UpdateBodyReference(World.Simulation.Bodies.GetBodyReference(this.BodyHandle));

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
                new CollidableDescription(ShapeHandle, 0.1f),
                new BodyActivityDescription(0.00f)
            ));

            ref var worldBodyProperties = ref World.BodyProperties.Allocate(BodyHandle);
            worldBodyProperties = new WorldBodyProperties
            {
                Friction = 0.5f
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

        public Vector2 Position
        {
            get
            {
                return Vector3ToVector2(BodyReference.Pose.Position);
            }
            set
            {
                BodyReference.Pose.Position = new Vector3(value.X, 0, value.Y);
            }
        }

        public Vector2 LinearVelocity
        {
            get
            {
                return Vector3ToVector2(BodyReference.Velocity.Linear);
            }
            set
            {
                BodyReference.Velocity.Linear = new Vector3(value.X, 0, value.Y);
            }
        }

        public float AngularVelocity
        {
            get
            {
                return -1 * BodyReference.Velocity.Angular.Y;
            }
            set
            {
                BodyReference.Velocity.Angular.Y = -1 * value;
            }
        }

        public float Angle
        {
            get
            {
                return -1 * ToEulerAngles(BodyReference.Pose.Orientation).Y;
            }
            set
            {
                BodyReference.Pose.Orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -value);
            }
        }

        protected int _size = 100;
        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (value != _size)
                {
                    _size = value;
                    UpdateSize();
                }
            }
        }
        private float _mass = 100f;
        public virtual float Mass
        {
            get
            {
                return _mass;
            }
            set
            {
                if (value != _mass)
                {
                    _mass = value;

                    UpdateInertia();
                }
            }
        }

        protected virtual void UpdateSize()
        {
            ref var shape = ref World.Simulation.Shapes.GetShape<Sphere>(ShapeHandle.Index);
            shape.Radius = _size;
        }

        protected virtual void UpdateInertia()
        {
            ref var shape = ref World.Simulation.Shapes.GetShape<Sphere>(ShapeHandle.Index);
            BodyReference.LocalInertia = GetBodyInertia(shape, _mass);
        }

        public void UpdateBodyReference(BodyReference bodyReference)
        {
            BodyReference = bodyReference;
            BodyReference.Awake = true;
            BodyReference.Pose.Position.Y = 0;
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

        }

        void IActor.Think()
        {
            if (Exists)
                this.Update();

            if (PendingDestruction)
            {
                Destroy();
                if (this.Exists)
                {
                    World.Actors.Remove(this);
                    World.BodyRemove(this);
                    World.Simulation.Bodies.Remove(this.BodyHandle);
                    World.Simulation.Shapes.Remove(this.ShapeHandle);
                    this.BodyHandle = default;
                    this.BodyReference = default;

                    this.ShapeHandle = default;
                    this.Exists = false;
                }
                PendingDestruction = false;
            }
        }

        protected virtual void Die()
        {
            this.PendingDestruction = true;
        }


        public virtual void Destroy()
        {
        }

        public virtual bool IsCollision(WorldBody otherBody)
        {
            return false;
        }

        public virtual void CollisionExecute(WorldBody projectedBody)
        {
            
        }
    }
}
