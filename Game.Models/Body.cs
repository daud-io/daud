namespace Game.Models
{
    using Newtonsoft.Json;
    using System.Numerics;

    public class Body
    {
        public long ID { get; set; }
        public long DefinitionTime { get; set; }

        [JsonIgnore]
        public bool Exists { get; set; }
        [JsonIgnore]
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

        private float _angle { get; set; }
        public virtual float Angle
        {
            get
            {
                return _angle;
            }
            set
            {
                IsDirty = IsDirty || _angle != value;
                _angle = value;
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
