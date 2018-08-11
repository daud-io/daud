namespace Game.Models
{
    using Newtonsoft.Json;
    using System.Numerics;

    public class ProjectedBody : Body
    {
        [JsonIgnore]
        public long ProjectedTime { get; set; }

        private Vector2 _position { get; set; } = new Vector2(0, 0);
        [JsonIgnore]
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

        public void Project(long time)
        {
            
            ProjectedTime = time;
            if (DefinitionTime == 0)
                DefinitionTime = time;

            var timeDelta = (time - this.DefinitionTime);

            _position = Vector2.Add(OriginalPosition, Vector2.Multiply(Momentum, timeDelta));
        }

        public ProjectedBody Clone()
        {
            return this.MemberwiseClone() as ProjectedBody;
        }
    }
}
