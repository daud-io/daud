namespace Game.Engine.Core
{
    using RBush;
    using System;
    using System.Numerics;

    public class Body : ISpatialData
    {
        public long ProjectedTime { get; set; }
        private Vector2 _position { get; set; } = new Vector2(0, 0);

        public Envelope Envelope;

        public uint ID { get; set; }
        public uint DefinitionTime { get; set; }
        public Group Group { get; set; }

        public bool Exists { get; set; }
        public bool IsDirty { get; set; } = true;


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

        private Vector2 _momentum = new Vector2(0, 0);
        public virtual Vector2 Momentum
        {
            get
            {
                return _momentum;
            }
            set
            {
                if (float.IsNaN(value.X))
                    throw new Exception("Invalid position");
                IsDirty = IsDirty || _momentum != value;
                _momentum = value;
            }
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
            ProjectedTime = time;
            if (DefinitionTime == 0)
                DefinitionTime = time;

            var timeDelta = (time - this.DefinitionTime);

            _position = Vector2.Add(OriginalPosition, Vector2.Multiply(Momentum, timeDelta));
            if (float.IsNaN(_position.X))
                throw new Exception("Invalid position");

            _angle = OriginalAngle + timeDelta * AngularVelocity;

            Envelope = new Envelope(_position.X - Size / 2, _position.Y - Size / 2, _position.X + Size / 2, _position.Y + Size / 2);
        }

        public Body Clone()
        {
            return this.MemberwiseClone() as Body;
        }
    }
}
