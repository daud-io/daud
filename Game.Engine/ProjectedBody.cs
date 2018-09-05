namespace Game.Engine
{
    using Game.Engine.Core;
    using System.Numerics;

    public class ProjectedBody : Body
    {
        public long ProjectedTime { get; set; }

        private Vector2 _position { get; set; } = new Vector2(0, 0);
        public virtual Vector2 Position
        {
            set
            {
                if (_position != value)
                {
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
                    _angle = value;
                    IsDirty = true;
                }
            }
            get
            {
                return _angle;
            }
        }

        public void Project(long time)
        {
            ProjectedTime = time;
            if (DefinitionTime == 0)
                DefinitionTime = time;

            var timeDelta = (time - this.DefinitionTime);

            _position = Vector2.Add(OriginalPosition, Vector2.Multiply(Momentum, timeDelta));
            _angle = OriginalAngle + timeDelta * AngularVelocity;
        }

        public ProjectedBody Clone()
        {
            return this.MemberwiseClone() as ProjectedBody;
        }
    }
}
