namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using System.Collections.Generic;

    public class ConfigurableContextBotConfig
    {
        public IEnumerable<BehaviorDescriptor> Behaviors { get; set; }
        public int Steps { get; set; } = 8;
        public bool RingDebugEnabled { get; set; } = false;
    }
}
