namespace Game.Engine.Core
{
    using BepuPhysics;
    using Game.API.Common;
    using RBush;
    using System;
    using System.Numerics;

    public class Body : ISpatialData
    {
        public long ProjectedTime { get; set; }
        private Vector2 _position { get; set; } = new Vector2(0, 0);
        protected long MaximumCleanTime = 2000;

        public Envelope Envelope;
        private bool ProjectedOnce = false;

        public uint ID { get; set; }
        public uint DefinitionTime { get; set; }
        public Group Group { get; set; }

        public bool Exists { get; set; }
        public bool IsDirty { get; set; } = true;

        public bool Indexed { get; set; } = false;
        public bool Removed { get; set; } = false;
        public bool Updated { get; set; } = false;

        public Vector2 IndexedPosition { get; set; }

        public bool IsStatic { get; set; } = false;

        private int _size { get; set; }
        public virtual int Size
        {
            get
            {
                return _size;
            }
            set
            {
                IsDirty = IsDirty || _size != value;
                _size = value;
            }
        }

        private byte _mode { get; set; }
        public virtual byte Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                IsDirty = IsDirty || _mode != value;
                _mode = value;
            }
        }

        private Sprites _sprite { get; set; }
        public virtual Sprites Sprite
        {
            get
            {
                return _sprite;
            }
            set
            {
                IsDirty = IsDirty || _sprite != value;
                _sprite = value;
            }
        }

        private string _color { get; set; }
        public virtual string Color
        {
            get
            {
                return _color;
            }
            set
            {
                IsDirty = IsDirty || _color != value;
                _color = value;
            }
        }

        private float _anuglarVelocity { get; set; }
        public virtual float AngularVelocity
        {
            get
            {
                return _anuglarVelocity;
            }
            set
            {
                IsDirty = IsDirty || _anuglarVelocity != value;
                _anuglarVelocity = value;
            }
        }

        private float _originalAngle { get; set; }
        public virtual float OriginalAngle
        {
            get
            {
                return _originalAngle;
            }
            set
            {
                IsDirty = IsDirty || _originalAngle != value;
                _originalAngle = value;
            }
        }

        internal BodyHandle BodyHandle;
        private Vector2 _linearVelocity = new Vector2(0, 0);
        public virtual Vector2 LinearVelocity
        {
            get
            {
                return _linearVelocity;
            }
            set
            {
                if (float.IsNaN(value.X) || float.IsNaN(value.Y))
                    throw new Exception("Invalid position");

                IsDirty = IsDirty || _linearVelocity != value;
                _linearVelocity = value;
            }
        }
        
        [Obsolete("Property renamed because of abuse of real physics terminology")]
        public virtual Vector2 Momentum
        {
            get => LinearVelocity;
            set => LinearVelocity = value;
        }

        private Vector2 _originalPosition { get; set; } = new Vector2(0, 0);
        public virtual Vector2 OriginalPosition
        {
            get
            {
                return _originalPosition;
            }
            set
            {
                IsDirty = IsDirty || _originalPosition != value;
                _originalPosition = value;
            }
        }

        public virtual Vector2 Position
        {
            set
            {
                if (_position != value)
                {
                    if (float.IsNaN(value.X))
                        throw new Exception("Invalid position");
                    _position = value;
                    IsDirty = true;
                }
            }
            get
            {
                return _position;
            }
        }

        private float _angle { get; set; } = 0;
        public virtual float Angle
        {
            set
            {
                if (_angle != value)
                {
                    if (float.IsNaN(value))
                        throw new Exception("Invalid angle");

                    _angle = value;
                    IsDirty = true;
                }
            }
            get
            {
                return _angle;
            }
        }

        ref readonly Envelope ISpatialData.Envelope => ref this.Envelope;

        public void Project(uint time)
        {
            if (IsStatic)
            {
                if (!ProjectedOnce)
                {
                    _position = _originalPosition;
                    _angle = _originalAngle;
                    Envelope = new Envelope(_position.X - Size, _position.Y - Size, _position.X + Size, _position.Y + Size);
                    ProjectedOnce = true;
                }
            }
            else
            {
                ProjectedTime = time;
                if (DefinitionTime == 0)
                    DefinitionTime = time;

                var timeDelta = (time - this.DefinitionTime);

                _position = Vector2.Add(OriginalPosition, Vector2.Multiply(LinearVelocity, timeDelta));

                _angle = OriginalAngle + timeDelta * AngularVelocity;

                if (time - this.DefinitionTime > MaximumCleanTime)
                    this.IsDirty = true;
            }
        }
        
        public void UpdateFromBodyReference(BodyReference bodyReference, uint time)
        {
            ProjectedTime = time;
            DefinitionTime = time;

            _position = Vector3ToVector2(bodyReference.Pose.Position);
            _originalPosition = _position;
            _linearVelocity = Vector3ToVector2(bodyReference.Velocity.Linear);
            _angle = ToEulerAngles(bodyReference.Pose.Orientation).Y;
            IsDirty = false;

        }

        static Vector3 ToEulerAngles(Quaternion q)
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

        public void UpdateBodyReference(BodyReference bodyReference, uint time)
        {
            DefinitionTime = time;
            OriginalPosition = Position;
            OriginalAngle = Angle;

            bodyReference.Pose.Position = new Vector3(Position.X, 0, Position.Y);
            bodyReference.Velocity.Linear = new Vector3(LinearVelocity.X, 0, LinearVelocity.Y);
                
            bodyReference.Pose.Orientation = Quaternion.CreateFromAxisAngle(new Vector3(0,1,0), Angle);
        }

        public Body Clone()
        {
            return this.MemberwiseClone() as Body;
        }
    }
}
