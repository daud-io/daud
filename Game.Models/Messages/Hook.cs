namespace Game.Models.Messages
{
    public class Hook : MessageBase
    {
        public override MessageTypes Type => MessageTypes.Hook;

        public static Hook Default
        {
            get
            {
                return new Hook
                {
                    BaseThrust = 0.15f,
                    HealthHitCost = 20,
                    MaxBoostTime = 100,
                    HealthRegenerationPerFrame = 0.3f,
                    MaxSpeed = 0.3f,
                    MaxSpeedBoost = 1f,
                    MaxHealth = 100,
                    ShootCooldownTime = 500,
                    ShootCooldownTimeBot = 800,
                    MaxHealthBot = 50,
                    BaseThrustBot = 0.10f,
                    BulletLife = 2000,
                    BulletSpeed = 1.2f,
                    BotPerXPoints = 500,
                    BotBase = 1,
                    StepTime = 40
                };
            }
        }

        public float BaseThrust { get; set; }
        public float BaseThrustBot { get; set; }

        public int HealthHitCost { get; set; }
        public int MaxBoostTime { get; set; }
        public float HealthRegenerationPerFrame { get; set; }
        public float MaxSpeed { get; set; }
        public float MaxSpeedBoost { get; set; }
        public int ShootCooldownTime { get; set; }
        public int ShootCooldownTimeBot { get; set; }
        public int MaxHealth { get; set; }
        public int MaxHealthBot { get; set; }
        public int BulletLife { get; set; }
        public float BulletSpeed { get; set; }
        public int BotBase { get; set; }
        public int BotPerXPoints { get; set; }

        public int StepTime { get; set; }
    }
}