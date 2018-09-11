namespace Game.Engine.Core
{
    using System.Numerics;

    public class ControlInput
    {
        public float Angle { get; set; }
        public Vector2 Position { get; set; }
        public bool BoostRequested { get; set; }
        public bool ShootRequested { get; set; }
    }
}