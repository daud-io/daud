namespace Game.API.Client
{
    using System.Numerics;

    public class ProjectedBody
    {
        public int ID { get; set; }
        public long DefinitionTime { get; set; }

        public float OriginalAngle { get; set; }
        public float Angle { get; set; }
        public float AngularVelocity { get; set; }

        public Vector2 OriginalPosition { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Momentum { get; set; }

        public int Size { get; set; }

        public string Sprite { get; set; }
        public string Caption { get; set; }
        public string Color { get; set; }
    }
}
