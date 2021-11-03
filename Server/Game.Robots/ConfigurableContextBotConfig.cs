namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using System.Collections.Generic;

    public class ConfigurableContextBotConfig
    {
        public IEnumerable<BehaviorDescriptor> Behaviors { get; set; }
        public int Steps { get; set; } = 8;
        public bool RingDebugEnabled { get; set; } = false;

        public string Name { get; set; } = null;
        public string Sprite { get; set; } = null;
        public string Color { get; set; } = null;
        public string RobotType { get; set; } = null;

        public List<string> Allies = null;

        public LevelingConfig Leveling { get; set; }

        public BlendingConfigModel BlendingConfig { get; set; }

        public class LevelingConfig
        {
            public object[] Levels { get; set; }
            public Dictionary<string, object> BehaviorModifications { get; set; }
            public int DownlevelThresholdMS { get; set; }
        }

        public class BlendingConfigModel
        {
            public int BlurSteps { get; set; } = 10;
            public float BlurAmount { get; set; } = 0.05f;
            public int BlurResolutionMultiplier { get; set; } = 1;

        }
    }
}
