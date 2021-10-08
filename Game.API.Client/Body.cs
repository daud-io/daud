namespace Game.API.Client
{
    using Game.API.Common;
    using System.Numerics;

    public class Body
    {
        public uint ID { get; set; }
        public long DefinitionTime { get; set; }

        public float OriginalAngle { get; set; }
        public float Angle { get; set; }
        public float AngularVelocity { get; set; }

        public Vector2 OriginalPosition { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        public int Size { get; set; }
        public Sprites Sprite { get; set; }

        public uint GroupID { get; set; }

        public BodyCache Cache { get; set; }

        public Group Group
        {
            get
            {
                return this.Cache?.GetGroup(this.GroupID);
            }
        }

        public void Project(long time)
        {
            var timeDelta = (time - this.DefinitionTime);

            Position = Vector2.Add(OriginalPosition, Vector2.Multiply(Velocity, timeDelta));

            Angle = OriginalAngle + timeDelta * AngularVelocity;
        }

        public Body ProjectNew(long time)
        {
            var newBody = this.Clone() as Body;
            newBody.Project(time);
            return newBody;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
