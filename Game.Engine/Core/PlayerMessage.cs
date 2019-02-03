namespace Game.Engine.Core
{
    public class PlayerMessage
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public int PointsDelta { get; set; }
        public object ExtraData { get; set; }
    }
}
