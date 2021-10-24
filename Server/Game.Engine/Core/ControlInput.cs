namespace Game.Engine.Core
{
    using System.Numerics;

    public class ControlInput
    {
        public Vector2 Position { get; set; }
        public bool BoostRequested { get; set; }
        public bool ShootRequested { get; set; }
        public string CustomData { get; set; }
    }
}