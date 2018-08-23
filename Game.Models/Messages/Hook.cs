namespace Game.Models.Messages
{
    public class Hook : MessageBase
    {
        public override MessageTypes Type => MessageTypes.Hook;

        public int BaseThrust { get; set; }
        public int BaseThrustBot { get; set; }
        public float HealthHitCost { get; set; }
        public int MaxBoostTime { get; set; }
        public float HealthRegenerationPerFrame { get; set; }
        public int MaxSpeed { get; set; }
        public int MaxSpeedBoost { get; set; }
        public int ShootCooldownTime { get; set; }
        public int ShootCooldownTimeBot { get; set; }
        public float MaxHealth { get; set; }
        public float MaxHealthBot { get; set; }
        public int BulletLife { get; set; }
        public int BulletSpeed { get; set; }
        public int BotBase { get; set; }
        public int BotPerXPoints { get; set; }
    }
}