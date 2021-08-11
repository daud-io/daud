namespace Game.Engine.Core
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using Game.API.Common;
    using System;
    using System.Numerics;

    public class Body : IActor
    {
        public readonly World World;
        public uint ID;

        internal BodyHandle BodyHandle;

        public Group Group;

        public bool Exists;

        public int Size = 0;
        public byte Mode = 0;
        public Sprites Sprite;
        public string Color;

        private BodyReference BodyReference;
        public bool PendingDestruction = false;


        public Body(World world)
        {
            this.World = world;

            this.ID = World.GenerateObjectID();
            int size = Math.Min(Math.Max(this.Size, 1), 1000);
            var capsule = new Capsule(size, 5000);
            var mass = 4 / 3 * MathF.PI * (size ^ 3) * 0.25f;
            capsule.ComputeInertia(mass, out var capsuleIntertia);
            capsuleIntertia.InverseInertiaTensor.XX = 0;
            capsuleIntertia.InverseInertiaTensor.YY = 0;

            var position2 = InitialPosition();
            
            BodyHandle = World.Simulation.Bodies.Add(BodyDescription.CreateDynamic(
                new Vector3(position2.X, 0, position2.Y),
                capsuleIntertia,
                new CollidableDescription(World.Simulation.Shapes.Add(capsule), 0.1f),
                new BodyActivityDescription(0.01f)
            ));

            this.UpdateBodyReference(World.Simulation.Bodies.GetBodyReference(this.BodyHandle));

            World.BodyAdd(this);
            World.Actors.Add(this);
            this.Exists = true;
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
                return BodyReference.Velocity.Angular.Y;

            }
            set
            {
                BodyReference.Velocity.Angular.Y = value;
            }
        }

        public float Angle
        {
            get
            {
                return ToEulerAngles(BodyReference.Pose.Orientation).Y;
            }
            set
            {
                BodyReference.Pose.Orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), value);
            }
        }

        public void UpdateBodyReference(BodyReference bodyReference)
        {
            BodyReference = bodyReference;
            BodyReference.Awake = true;
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
            if (PendingDestruction)
            {
                Destroy();
                if (this.Exists)
                {
                    World.Actors.Remove(this);
                    World.BodyRemove(this);
                    World.Simulation.Bodies.Remove(this.BodyHandle);
                    this.BodyHandle = default;
                    this.BodyReference = default;
                    this.Exists = false;
                }
                PendingDestruction = false;
            }

            if (Exists)
                this.Update();
        }

        protected virtual void Die()
        {
            this.PendingDestruction = true;
        }


        public virtual void Destroy()
        {
        }


        protected virtual void Collided(ICollide otherObject)
        {

        }

        // this doesn't do what you think it does.
        // it's only useful for caching.
        public Body Clone()
        {
            return this.MemberwiseClone() as Body;
        }
    }
}
