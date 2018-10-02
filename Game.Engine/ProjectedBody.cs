namespace Game.Engine
{
    using Game.Engine.Core;
    using RBush;
    using System;
    using System.Numerics;

    public class ProjectedBody : Body, ISpatialData
    {
        public long ProjectedTime { get; set; }
        private Vector2 _position { get; set; } = new Vector2(0, 0);

        public Envelope Envelope;

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

        public void Project(long time)
        {
            ProjectedTime = time;
            if (DefinitionTime == 0)
                DefinitionTime = time;

            var timeDelta = (time - this.DefinitionTime);

            _position = Vector2.Add(OriginalPosition, Vector2.Multiply(Momentum, timeDelta));
            if (float.IsNaN(_position.X))
                throw new Exception("Invalid position");

            _angle = OriginalAngle + timeDelta * AngularVelocity;

            Envelope = new Envelope(_position.X - Size/2, _position.Y - Size / 2, _position.X + Size / 2, _position.Y + Size / 2);
        }

        public ProjectedBody Clone()
        {
            return this.MemberwiseClone() as ProjectedBody;
        }
    }
}
