namespace Game.Engine.Core
{
    public class Hook
    {
        public static Hook Default
        {
            get
            {
                return new Hook
                {
                    BaseThrust = 0.075f,
                    HealthHitCost = 100,
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
                    BotBase = 4,
                    StepTime = 40,
                    Obstacles = 6,
                    ObstacleMaxMomentum = 0.1f,
                    ObstacleMaxSize = 1000,
                    TeamMode = true,

                    FlockAlignment = 0.0f,
                    FlockCohesion = 0.002f,
                    FlockCohesionMaximumDistance = 2000,
                    FlockSeparation = 40f,
                    FlockSeparationMinimumDistance = 200,
                    FlockWeight = 0.14f,

                    FlockSpeed = 0
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
        public int Obstacles { get; set; }
        public float ObstacleMaxMomentum { get; set; }
        public int ObstacleMaxSize { get; set; }

        public bool TeamMode { get; set; }


        public float FlockAlignment { get; set; }
        public float FlockCohesion { get; set; }
        public int FlockCohesionMaximumDistance { get; set; }
        public float FlockSeparation { get; set; }
        public int FlockSeparationMinimumDistance { get; set; }
        public float FlockWeight { get; set; }
        public int FlockSpeed { get; set; }



        public int StepTime { get; set; }
    }
}