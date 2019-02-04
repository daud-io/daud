namespace Game.API.Common.Models
{
    using Game.API.Common;
    using System.Numerics;

    public class Hook
    {
        public static Hook Default
        {
            get
            {
                return new Hook
                {
                    WorldSize = 8000,
                    FollowFirstShip = false,
                    FiringSequenceDelay = 0,

                    BaseThrustM = -0.0035f,
                    BaseThrustB = 0.155f,

                    Drag = 0.92f,

                    BoostThrust = 0.15f,

                    BoostCooldownTimeM = 14.0f,
                    BoostCooldownTimeB = 1080.0f,
                    ShotCooldownTimeShark = 300,

                    BoostSpeed = 1f,
                    BoostDuration = 420,

                    ShotCooldownTimeM = 20,
                    ShotCooldownTimeB = 550,

                    ShotCooldownTimeBotM = 21,
                    ShotCooldownTimeBotB = 550,

                    ShotThrustM = -0.004f,
                    ShotThrustB = 0.2f,

                    SeekerThrustMultiplier = 1.35f,
                    SeekerLifeMultiplier = 1.15f,

                    HealthHitCost = 100,
                    HealthRegenerationPerFrame = 0.0f,
                    MaxHealth = 100,

                    MaxHealthBot = 50,
                    BulletLife = 1890,
                    BotPerXPoints = 500,
                    BotBase = 1,
                    BotRespawnDelay = 10000,

                    StepTime = 40,
                    Wormholes = 0,
                    WormholesDestination = null,

                    Obstacles = 10,
                    ObstacleMaxMomentum = 0.1f,
                    ObstacleMaxMomentumWeatherMultiplier = 1.0f,
                    ObstacleMinSize = 300,
                    ObstacleMaxSize = 600,
                    ObstacleBorderBuffer = 1000,

                    TeamMode = false,
                    CTFMode = false,
                    CTFCarryBurden = 0.2f,
                    CTFSpawnDistance = 6000,

                    SumoMode = false,
                    SumoRingSize = 1000,

                    SpawnShipCount = 5,
                    SpawnInvulnerabilityTime = 3000,

                    FlockAlignment = .35f,
                    FlockCohesion = 0.006f,
                    FlockCohesionMaximumDistance = 600,
                    FlockSeparation = 80f,
                    FlockSeparationMinimumDistance = 200,
                    FlockWeight = 0.14f,
                    SnakeWeight = 0f,
                    BossMode = false,

                    ShipGainBySizeM = -0.034f,
                    ShipGainBySizeB = 1.03f,

                    FlockSpeed = 0,

                    PickupSeekers = 6,
                    PickupShields = 4,
                    ShieldStrength = 2,

                    Fishes = 60,
                    FishThrust = 0.04f,

                    SeekerRange = 2100,
                    SeekerDelay = 250,

                    SpawnLocationMode = "QuietSpot", // Corners, QuietSpot, Static
                    SpawnLocation = Vector2.Zero,

                    PointsPerKillShip = 1,
                    PointsPerKillFleet = 55,
                    PointsPerUniverseDeath = -1,
                    PointsMultiplierDeath = 0.5f,

                    PlayerCountGracePeriodMS = 15000,
                    FleetWeaponStackDepth = 1,

                    LifecycleDuration = 10000,
                    MapEnabled = false,

                    AllowedColors = AllColors,
                    Name = "FFA"
                };
            }
        }


        public int WorldSize { get; set; }

        public float BaseThrustM { get; set; }
        public float BaseThrustB { get; set; }

        public float BoostThrust { get; set; }

        public float BoostCooldownTimeM { get; set; }
        public float BoostCooldownTimeB { get; set; }

        public int BoostDuration { get; set; }
        public float BoostSpeed { get; set; }

        public float Drag { get; set; }

        public int PointsPerKillShip { get; set; }
        public int PointsPerKillFleet { get; set; }
        public int PointsPerUniverseDeath { get; set; }
        public float PointsMultiplierDeath { get; set; }

        public int HealthHitCost { get; set; }
        public float HealthRegenerationPerFrame { get; set; }

        public int SpawnShipCount { get; set; }

        public float ShotCooldownTimeM { get; set; }
        public float ShotCooldownTimeB { get; set; }
        public int ShotCooldownTimeShark { get; set; }

        public float ShotCooldownTimeBotM { get; set; }
        public float ShotCooldownTimeBotB { get; set; }

        public float ShotThrustM { get; set; }
        public float ShotThrustB { get; set; }

        public int MaxHealth { get; set; }
        public int MaxHealthBot { get; set; }
        public float SeekerThrustMultiplier { get; set; }
        public int BulletLife { get; set; }
        public float SeekerLifeMultiplier { get; set; }
        public int BotBase { get; set; }
        public int BotPerXPoints { get; set; }

        public int Wormholes { get; set; }
        public string WormholesDestination { get; set; }

        public int Obstacles { get; set; }
        public int PickupSeekers { get; set; } = 0;
        public int Fishes { get; set; } = 0;
        public float FishThrust { get; set; } = 0;

        public float ObstacleMaxMomentum { get; set; }
        public int ObstacleMinSize { get; set; }
        public int ObstacleMaxSize { get; set; }
        public float ObstacleMaxMomentumWeatherMultiplier { get; set; }
        public int ObstacleBorderBuffer { get; set; }

        public bool TeamMode { get; set; }
        public bool CTFMode { get; set; }
        public float CTFCarryBurden { get; set; }
        public int CTFSpawnDistance { get; set; }
        public bool SumoMode { get; set; }
        public int SumoRingSize { get; set; }

        public int LeaderboardRefresh { get; set; } = 750;

        public float FlockAlignment { get; set; }
        public float FlockCohesion { get; set; }
        public int FlockCohesionMaximumDistance { get; set; }
        public float FlockSeparation { get; set; }
        public int FlockSeparationMinimumDistance { get; set; }
        public float FlockWeight { get; set; }
        public float SnakeWeight { get; set; }
        public bool BossMode { get; set; }
        public Sprites[] BossModeSprites { get; set; }

        public int FlockSpeed { get; set; }

        public int SeekerRange { get; set; }
        public int SeekerDelay { get; set; }

        public float ShipGainBySizeM { get; set; }
        public float ShipGainBySizeB { get; set; }

        public bool MapEnabled { get; set; }


        public int StepTime { get; set; }
        public float OutOfBoundsDeathLine { get; set; } = 800;
        public float OutOfBoundsBorder { get; set; } = 300;
        public float OutOfBoundsDecayDistance { get; set; } = 900;
        public int BotRespawnDelay { get; set; }
        public int PickupShields { get; set; }
        public int ShieldStrength { get; set; }

        public bool FollowFirstShip { get; set; }
        public int FiringSequenceDelay { get; set; }

        public string SpawnLocationMode { get; set; }
        public Vector2 SpawnLocation { get; set; }

        public int LifecycleDuration {get;set;}

        public int PlayerCountGracePeriodMS { get; set; }
        public int FleetWeaponStackDepth { get; set; }
        public int SpawnInvulnerabilityTime { get; set; }


        public static readonly string[] AllColors = new[] {
            "ship_pink",
            "ship_red",
            "ship_orange",
            "ship_yellow",
            "ship_green",
            "ship_cyan"
        };
        public static readonly string[] TeamColors = new[] {
            "ship_red",
            "ship_cyan"
        };

        public string Name { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }

        public bool Hidden { get; set; }
        public string[] AllowedColors { get; set; }

        public int Weight { get; set; }


        public Hook Clone()
        {
            return this.MemberwiseClone() as Hook;
        }
    }
}