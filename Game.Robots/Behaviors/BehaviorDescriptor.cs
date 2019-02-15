namespace Game.Robots.Behaviors
{
    public class BehaviorDescriptor
    {
        public string BehaviorTypeName { get; set; }
        public int LookAheadMS { get; set; }
        public float BehaviorWeight { get; set; }
        public int Cycle { get; set; }
        public bool Plot { get; set; }
        public object Config { get; set; }
    }
}
