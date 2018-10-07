namespace Game.Engine.Core
{
    using Newtonsoft.Json;
    using System;
    using System.Numerics;

    public class Body
    {
        public uint ID { get; set; }
        public uint DefinitionTime { get; set; }

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

        private string _sprite { get; set; }
        public virtual string Sprite
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

        private string _caption { get; set; }
        public virtual string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                IsDirty = IsDirty || _caption != value;
                _caption = value;
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

        private Vector2 _momentum  = new Vector2(0, 0);
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
    }
}
