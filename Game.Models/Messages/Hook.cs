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
                    BaseThrust = 6,
                    HealthHitCost = 20,
                    MaxBoostTime = 100,
                    HealthRegenerationPerFrame = 0.3f,
                    MaxSpeed = 12,
                    MaxSpeedBoost = 40,
                    MaxHealth = 100,
                    ShootCooldownTime = 500,
                    ShootCooldownTimeBot = 800,
                    MaxHealthBot = 50,
                    BaseThrustBot = 2,
                    BulletLife = 2000,
                    BulletSpeed = 50,
                    BotPerXPoints = 500,
                    BotBase = 1,
                    StepTime = 40
                };
            }
        }

        public int BaseThrust { get; set; }
        public int BaseThrustBot { get; set; }

        public int HealthHitCost { get; set; }
        public int MaxBoostTime { get; set; }
        public float HealthRegenerationPerFrame { get; set; }
        public int MaxSpeed { get; set; }
        public int MaxSpeedBoost { get; set; }
        public int ShootCooldownTime { get; set; }
        public int ShootCooldownTimeBot { get; set; }
        public int MaxHealth { get; set; }
        public int MaxHealthBot { get; set; }
        public int BulletLife { get; set; }
        public int BulletSpeed { get; set; }
        public int BotBase { get; set; }
        public int BotPerXPoints { get; set; }

        public int StepTime { get; set; }
    }
}