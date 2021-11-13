namespace Game.API.Client
{
    using Game.API.Common;
    using System.Numerics;

    public class Body
    {
        public uint ID;
        public long DefinitionTime;

        public float OriginalAngle;
        public float Angle;
        public float AngularVelocity;

        public Vector2 OriginalPosition;
        public Vector2 Position;
        public Vector2 Velocity;

        public int Size;
        public Sprites Sprite;

        public uint GroupID;

        public BodyCache Cache;

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
