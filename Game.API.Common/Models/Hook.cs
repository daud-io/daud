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
                    WorldSize = 5500,
                    WorldResizeEnabled = false,
                    WorldSizeBasic = 5500,
                    WorldSizeDeltaPerPlayer = 400,
                    WorldResizeSpeed = 5,
                    WorldMinPlayersToResize = 4,
                    
                    // sizes
                    BulletSize = 5,
                    // ShipSize = 10,

                    FollowFirstShip = false,
                    FiringSequenceDelay = 0,

                    EarnedShipDelay = 0,

                    BaseThrustM = -0.00015f,
                    BaseThrustB = 0.015f,
                    // values below probably need to be adjusted
                    BaseThrust = new[] {
                        0,
                        14, 17, 16, 15, 15, 14, 14, 14, 14, 13, 13,
                        13, 13, 13, 13, 12, 12, 12, 12, 12, 12, 12,
                        12, 12, 12, 12, 12, 11, 11, 11, 11, 11, 11,
                        11, 11, 11, 11, 11, 11, 11, 11, 11, 10, 10,
                        10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
                        10, 10, 9, 9, 9, 9, 9, 9, 9, 9,
                        9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
                        9, 9, 9, 9, 9, 9, 9, 9, 8, 8,
                        8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
                        8, 8, 8, 8, 8, 8, 8, 8, 8, 8
                    },
                    BaseThrustConverter = 0.00113f,

                    Drag = 0.92f,

                    BoomDrag = 0.92f,
                    BoomLife = 500,

                    BoostThrust = 0.0001f,

                    BoostCooldownTimeM = 14.0f,
                    BoostCooldownTimeB = 1080.0f,
                    ShotCooldownTimeShark = 300,

                    BoostSpeed = 0.3f,
                    BoostDuration = 420,

                    AbandonBuffer = 120,

                    ShotCooldownTimeM = 20,
                    ShotCooldownTimeB = 550,

                    ShotCooldownTimeBotM = 22,
                    ShotCooldownTimeBotB = 1100,

                    ShotThrustM = -0.001f,
                    ShotThrustB = 0.045f,
                    ShotThrust = new[] {
                        0, // 0 size
                        45, 45, 36, 36, 36, 36, 36, 36, 27, 27, // 1 - 10 size
                        27, 27, 27, 22, 22, 22, 20, 20, 20, 18, // 11 - 20 size
                        18, 18, 18, 17, 17, 17, 17, 17, 16, 16, // 21 - 30 size
                        16, 15, 15, 15, 15, 15, 15, 15, 15, 15, // 31 - 40 size
                        15, 13, 13, 12, 12, 12, 12, 12, 12, 12, // 41 - 50 size
                        12, 11, 11, 11, 11, 11, 11, 11, 10, 10, // 51 - 60 size
                        10, 10, 10, 10, 10, 10, 10, 10, 10, 10, // 61 - 70 size
                        10, 10, 10, 10, 10, 10, 10, 10, 10, 10, // 71 - 80 size
                        10, 10, 10, 10, 10, 10, 10, 10, 10, 10, // 81 - 90 size
                        10, 10, 10, 10, 10, 10, 10, 10, 10, 10, // 91 - 100 size
                    },
                    ShotThrustConverter = 0.00113f,

                    SeekerThrustMultiplier = 1.35f,
                    SeekerLifeMultiplier = 1.15f,

                    HealthHitCost = 100,
                    HealthRegenerationPerFrame = 0.0f,
                    MaxHealth = 100,

                    MaxHealthBot = 50,
                    BulletLife = 1500,
                    
                    BulletLifeB = 1000,
                    BulletLifeM = 25,
                    
                    BotPerXPoints = 500,
                    BotBase = 10,
                    BotRespawnDelay = 10000,
                    BotMaxRespawnDelay = 60000,

                    StepTime = 40,
                    Wormholes = 0,
                    WormholesDestination = null,

                    Obstacles = 0, // ignored if WorldResizeEnabled = true
                    ObstaclesMultiplier = 0.0005, // used when WorldResizeEnabled = true
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

                    SpawnShipCount = 3,
                    SpawnInvulnerabilityTime = 3000,

                    Fishes = 512, // ignored if WorldResizeEnabled = true
                    FishesMultiplier = 0.01, // used when WorldResizeEnabled = true
                    FishThrust = 0.003f,
                    FishFlockAlignment = 10f,
                    FishFlockCohesion = 0.001f,
                    FishFlockCohesionMaximumDistance = 3000,
                    FishFlockSeparation = 80,
                    FishFlockSeparationMinimumDistance = 300,
                    FishFlockWeight = 0.9f,
                    FishOOBWeight = 0.8f,
                    FishCycle = 500, // how often do they think

                    FlockAlignment = 0.5f,
                    FlockCohesion = 0.0005f,
                    FlockCohesionMaximumDistance = 1000,
                    FlockSeparation = 2f,
                    FlockSeparationMinimumDistance = 14,
                    FlockWeight = 0.5f,
                    SnakeWeight = 0f,
                    BossMode = false,

                    ShipGainBySizeM = -0.015f,
                    ShipGainBySizeB = 1.5f,

                    FlockSpeed = 0,

                    PickupShields = 0,
                    PickupShieldsMultiplier = 0,
                    ShieldStrength = 3,

                    PickupSeekers = 0,
                    PickupSeekersMultiplier = 0,
                    SeekerRange = 2100,
                    SeekerCycle = 250,
                    SeekerLead = 150,
                    SeekerNegotiation = true,

                    SpawnLocationMode = "QuietSpot", // Corners, QuietSpot, Static
                    SpawnLocation = Vector2.Zero,

                    PointsPerKillShip = 1,
                    PointsPerUniverseDeath = -1,
                    PointsMultiplierDeath = 0.5f,
                    PointsPerKillFleet = 55, // to resolve some problems in Worlds.cs
                    PointsPerKillFleetMax = 55,
                    PointsPerKillFleetStep = 5,
                    PointsPerKillFleetPerStep = 50,
                    ComboDelay = 4000,
                    ComboPointsStep = 5,

                    PlayerCountGracePeriodMS = 15000,
                    FleetWeaponStackDepth = 1,

                    LifecycleDuration = 10000,
                    MapEnabled = false,

                    AllowedColors = AllColors,
                    Name = "FFA",

                    LeaderboardRefresh = 30,

                    MaxNameLength = 15,
                    
                    PrecisionBullets = true,
                    PrecisionBulletsMinimumRange = 16384f
                };
            }
        }


        public int WorldSize { get; set; }
        public bool WorldResizeEnabled { get; set; }
        public int WorldResizeSpeed { get; set; }
        public int WorldSizeBasic { get; set; }
        public int WorldSizeDeltaPerPlayer { get; set; }
        public int WorldMinPlayersToResize { get; set; }
        
        public int BulletSize { get; set; }
        // public int ShipSize { get; set; }

        public float BaseThrustM { get; set; }
        public float BaseThrustB { get; set; }
        public int[] BaseThrust { get; set; }
        public float BaseThrustConverter { get; set; }

        public float BoostThrust { get; set; }

        public float BoostCooldownTimeM { get; set; }
        public float BoostCooldownTimeB { get; set; }

        public int BoostDuration { get; set; }
        public float BoostSpeed { get; set; }

        public float Drag { get; set; }

        public int BoomLife { get; set; }
        public float BoomDrag { get; set; }

        public int PointsPerKillShip { get; set; }
        public int PointsPerUniverseDeath { get; set; }
        public float PointsMultiplierDeath { get; set; }
        public int PointsPerKillFleet { get; set; }
        public int PointsPerKillFleetMax { get; set; }
        public int PointsPerKillFleetStep { get; set; }
        public float PointsPerKillFleetPerStep { get; set; }
        public int ComboDelay { get; set; }
        public int ComboPointsStep { get; set; }

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
        public int[] ShotThrust { get; set; }
        public float ShotThrustConverter { get; set; }

        public int MaxHealth { get; set; }
        public int MaxHealthBot { get; set; }
        public float SeekerThrustMultiplier { get; set; }
        public float PrecisionBulletsNoise { get; set; }
        public bool PrecisionBullets { get; set; }
        public float PrecisionBulletsMinimumRange { get; set; }

        public int ShieldCannonballLife { get; set; }
        public int BulletLife { get; set; }
        public int BulletLifeB { get; set; }
        public int BulletLifeM { get; set; }
        public float SeekerLifeMultiplier { get; set; }
        public int BotBase { get; set; }
        public int BotPerXPoints { get; set; }

        public int Wormholes { get; set; }
        public string WormholesDestination { get; set; }

        public int Obstacles { get; set; }
        public double PickupShieldsMultiplier { get; set; }

        public double ObstaclesMultiplier { get; set; }
        public float ObstacleMaxMomentum { get; set; }
        public int ObstacleMinSize { get; set; }
        public int ObstacleMaxSize { get; set; }
        public float ObstacleMaxMomentumWeatherMultiplier { get; set; }
        public bool ObstaclesSpawnShieldCannons { get; set; }
        public int ObstacleBorderBuffer { get; set; }

        public bool TeamMode { get; set; }
        public bool CTFMode { get; set; }
        public float CTFCarryBurden { get; set; }
        public int CTFSpawnDistance { get; set; }
        public bool SumoMode { get; set; }
        public int SumoRingSize { get; set; }

        public int LeaderboardRefresh { get; set; }

        public int Fishes { get; set; } = 0;
        public double FishesMultiplier { get; set; }
        public float FishThrust { get; set; } = 0;
        public float FishFlockAlignment { get; set; }
        public float FishFlockCohesion { get; set; }
        public int FishFlockCohesionMaximumDistance { get; set; }
        public float FishFlockSeparation { get; set; }
        public int FishFlockSeparationMinimumDistance { get; set; }
        public float FishFlockWeight { get; set; }
        public float FishOOBWeight { get; set; }
        public int FishCycle { get; set; }


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

        public int PickupSeekers { get; set; } = 0;
        public int PickupRobotGuns { get; set; } = 0;

        public double PickupSeekersMultiplier { get; set; }
        public bool SeekerNegotiation { get; set; }
        public int SeekerLead { get; set; }
        public int SeekerRange { get; set; }
        public int SeekerCycle { get; set; }

        public float ShipGainBySizeM { get; set; }
        public float ShipGainBySizeB { get; set; }

        public bool MapEnabled { get; set; }


        public int StepTime { get; set; }
        public float OutOfBoundsDeathLine { get; set; } = 800;
        public float OutOfBoundsBorder { get; set; } = 0;
        public float OutOfBoundsDecayDistance { get; set; } = 4000;
        public int BotRespawnDelay { get; set; }
        public int BotMaxRespawnDelay { get; set; }
        public int PickupShields { get; set; }
        public int ShieldStrength { get; set; }

        public bool FollowFirstShip { get; set; }
        public int FiringSequenceDelay { get; set; }

        public string SpawnLocationMode { get; set; }
        public Vector2 SpawnLocation { get; set; }

        public int LifecycleDuration { get; set; }

        public int PlayerCountGracePeriodMS { get; set; }
        public int FleetWeaponStackDepth { get; set; }
        public int SpawnInvulnerabilityTime { get; set; }


        public static readonly string[] AllColors = new[] {
            "ship_pink",
            "ship_red",
            "ship_orange",
            "ship_yellow",
            "ship_green",
            "ship_cyan",
            "ship_blue"
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
        public int MaxNameLength { get; set; }
        public string GearheadName { get; set; }
        public float GearheadRegen { get; set; }
        public int AutoRemoveOnEmptyThreshold { get; set; }
        public uint ExplosionTime { get; set; }
        public int AbandonBuffer { get; set; }
        public int EarnedShipDelay { get; set; }

        public Hook Clone()
        {
            return this.MemberwiseClone() as Hook;
        }
    }
}