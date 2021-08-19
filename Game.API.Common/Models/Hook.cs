namespace Game.API.Common.Models
{
    using Game.API.Common;
    using System;
    using System.Collections.Generic;
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
                    WorldResizeEnabled = true,
                    WorldSizeBasic = 8000,
                    WorldSizeDeltaPerPlayer = 400,
                    WorldResizeSpeed = 5,
                    WorldMinPlayersToResize = 4,

                    FollowFirstShip = false,
                    FiringSequenceDelay = 0,

                    EarnedShipDelay = 0,

                    BaseThrustM = -0.0035f,
                    BaseThrustB = 0.15f,

                    Drag = 0.92f,

                    BoomDrag = 0.92f,
                    BoomLife = 500,

                    BoostThrust = 0.15f,

                    BoostCooldownTimeM = 14.0f,
                    BoostCooldownTimeB = 1080.0f,
                    ShotCooldownTimeShark = 300,

                    BoostSpeed = 1f,
                    BoostDuration = 420,

                    AbandonBuffer = 120,

                    ShotCooldownTimeM = 20,
                    ShotCooldownTimeB = 550,

                    ShotCooldownTimeBotM = 14,
                    ShotCooldownTimeBotB = 1080,

                    ShotThrustM = -0.006f,
                    ShotThrustB = 0.22f,

                    SeekerThrustMultiplier = 1.35f,
                    SeekerLifeMultiplier = 1.15f,

                    HealthHitCost = 100,
                    HealthRegenerationPerFrame = 0.0f,
                    MaxHealth = 100,

                    MaxHealthBot = 50,
                    PrecisionBullets = false,
                    BulletLife = 1500,
                    BotPerXPoints = 500,
                    BotBase = 1,
                    BotRespawnDelay = 10000,
                    BotMaxRespawnDelay = 60000,

                    StepTime = 40,
                    Wormholes = 0,
                    WormholesDestination = null,

                    Obstacles = 10, // ignored if WorldResizeEnabled = true
                    ObstaclesMultiplier = 0.0005, // used when WorldResizeEnabled = true
                    ObstacleMaxMomentum = 0.1f,
                    ObstacleMaxMomentumWeatherMultiplier = 1.0f,
                    ObstacleMinSize = 50,
                    ObstacleMaxSize = 200,
                    ObstacleBorderBuffer = 1000,

                    TeamMode = false,
                    CTFMode = false,
                    CTFCarryBurden = 0.2f,
                    CTFSpawnDistance = 6000,

                    SumoMode = false,
                    SumoRingSize = 1000,

                    SpawnShipCount = 5,
                    SpawnInvulnerabilityTime = 3000,

                    Fishes = 60, // ignored if WorldResizeEnabled = true
                    FishesMultiplier = 0.01, // used when WorldResizeEnabled = true
                    FishThrust = 0.08f,
                    FishFlockAlignment = 5f,
                    FishFlockCohesion = 0.01f,
                    FishFlockCohesionMaximumDistance = 1000,
                    FishFlockSeparation = 50,
                    FishFlockSeparationMinimumDistance = 200,
                    FishFlockWeight = 1,
                    FishOOBWeight = 10,
                    FishCycle = 300, // how often do they think

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

                    PickupShields = 4,
                    PickupShieldsMultiplier = 0.0004,
                    ShieldStrength = 3,

                    PickupSeekers = 6,
                    PickupSeekersMultiplier = 0.0006,
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

                    LeaderboardRefresh = 750,

                    MaxNameLength = 17,


                    RoyaleMode = false,
                    RoyaleCountdownDurationSeconds = 5,
                    RoyaleResizeSpeed = 4,
                    RoyaleDoubleStep1 = 4200,
                    RoyaleDoubleStep2 = 1500,

                    EinsteinCoefficient = 0.25f
                };
            }
        }


        public int WorldSize { get; set; }
        public string WorldMesh { get; set; }
        public bool WorldResizeEnabled { get; set; }
        public int WorldResizeSpeed { get; set; }
        public int WorldSizeBasic { get; set; }
        public int WorldSizeDeltaPerPlayer { get; set; }
        public int WorldMinPlayersToResize { get; set; }

        public float BaseThrustM { get; set; }
        public float BaseThrustB { get; set; }

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

        public int MaxHealth { get; set; }
        public int MaxHealthBot { get; set; }
        public float SeekerThrustMultiplier { get; set; }
        public float PrecisionBulletsNoise { get; set; }
        public bool PrecisionBullets { get; set; }
        public float PrecisionBulletsMinimumRange { get; set; }

        public int ShieldCannonballLife { get; set; }
        public int BulletLife { get; set; }
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
        public bool RoyaleMode { get; set; }
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
            "ship_blue",
            "ship_secret",
            "ship_zed"
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
        public int RoyaleCountdownDurationSeconds { get; set; }
        public int RoyaleResizeSpeed { get; set; }
        public int RoyaleDoubleStep1 { get; set; }
        public int RoyaleDoubleStep2 { get; set; }
        public bool CanSpawn { get; set; } = true;
        public float EinsteinCoefficient { get; set; }
        public int Tokens { get; set; }

        public Hook Clone()
        {
            return this.MemberwiseClone() as Hook;
        }

        public override bool Equals(object obj)
        {
            return obj is Hook hook &&
                   WorldSize == hook.WorldSize &&
                   WorldResizeEnabled == hook.WorldResizeEnabled &&
                   WorldResizeSpeed == hook.WorldResizeSpeed &&
                   WorldSizeBasic == hook.WorldSizeBasic &&
                   WorldSizeDeltaPerPlayer == hook.WorldSizeDeltaPerPlayer &&
                   WorldMinPlayersToResize == hook.WorldMinPlayersToResize &&
                   BaseThrustM == hook.BaseThrustM &&
                   BaseThrustB == hook.BaseThrustB &&
                   BoostThrust == hook.BoostThrust &&
                   BoostCooldownTimeM == hook.BoostCooldownTimeM &&
                   BoostCooldownTimeB == hook.BoostCooldownTimeB &&
                   BoostDuration == hook.BoostDuration &&
                   BoostSpeed == hook.BoostSpeed &&
                   Drag == hook.Drag &&
                   BoomLife == hook.BoomLife &&
                   BoomDrag == hook.BoomDrag &&
                   PointsPerKillShip == hook.PointsPerKillShip &&
                   PointsPerUniverseDeath == hook.PointsPerUniverseDeath &&
                   PointsMultiplierDeath == hook.PointsMultiplierDeath &&
                   PointsPerKillFleet == hook.PointsPerKillFleet &&
                   PointsPerKillFleetMax == hook.PointsPerKillFleetMax &&
                   PointsPerKillFleetStep == hook.PointsPerKillFleetStep &&
                   PointsPerKillFleetPerStep == hook.PointsPerKillFleetPerStep &&
                   ComboDelay == hook.ComboDelay &&
                   ComboPointsStep == hook.ComboPointsStep &&
                   HealthHitCost == hook.HealthHitCost &&
                   HealthRegenerationPerFrame == hook.HealthRegenerationPerFrame &&
                   SpawnShipCount == hook.SpawnShipCount &&
                   ShotCooldownTimeM == hook.ShotCooldownTimeM &&
                   ShotCooldownTimeB == hook.ShotCooldownTimeB &&
                   ShotCooldownTimeShark == hook.ShotCooldownTimeShark &&
                   ShotCooldownTimeBotM == hook.ShotCooldownTimeBotM &&
                   ShotCooldownTimeBotB == hook.ShotCooldownTimeBotB &&
                   ShotThrustM == hook.ShotThrustM &&
                   ShotThrustB == hook.ShotThrustB &&
                   MaxHealth == hook.MaxHealth &&
                   MaxHealthBot == hook.MaxHealthBot &&
                   SeekerThrustMultiplier == hook.SeekerThrustMultiplier &&
                   PrecisionBulletsNoise == hook.PrecisionBulletsNoise &&
                   PrecisionBullets == hook.PrecisionBullets &&
                   PrecisionBulletsMinimumRange == hook.PrecisionBulletsMinimumRange &&
                   ShieldCannonballLife == hook.ShieldCannonballLife &&
                   BulletLife == hook.BulletLife &&
                   SeekerLifeMultiplier == hook.SeekerLifeMultiplier &&
                   BotBase == hook.BotBase &&
                   BotPerXPoints == hook.BotPerXPoints &&
                   Wormholes == hook.Wormholes &&
                   WormholesDestination == hook.WormholesDestination &&
                   Obstacles == hook.Obstacles &&
                   PickupShieldsMultiplier == hook.PickupShieldsMultiplier &&
                   ObstaclesMultiplier == hook.ObstaclesMultiplier &&
                   ObstacleMaxMomentum == hook.ObstacleMaxMomentum &&
                   ObstacleMinSize == hook.ObstacleMinSize &&
                   ObstacleMaxSize == hook.ObstacleMaxSize &&
                   ObstacleMaxMomentumWeatherMultiplier == hook.ObstacleMaxMomentumWeatherMultiplier &&
                   ObstaclesSpawnShieldCannons == hook.ObstaclesSpawnShieldCannons &&
                   ObstacleBorderBuffer == hook.ObstacleBorderBuffer &&
                   TeamMode == hook.TeamMode &&
                   RoyaleMode == hook.RoyaleMode &&
                   CTFMode == hook.CTFMode &&
                   CTFCarryBurden == hook.CTFCarryBurden &&
                   CTFSpawnDistance == hook.CTFSpawnDistance &&
                   SumoMode == hook.SumoMode &&
                   SumoRingSize == hook.SumoRingSize &&
                   LeaderboardRefresh == hook.LeaderboardRefresh &&
                   Fishes == hook.Fishes &&
                   FishesMultiplier == hook.FishesMultiplier &&
                   FishThrust == hook.FishThrust &&
                   FishFlockAlignment == hook.FishFlockAlignment &&
                   FishFlockCohesion == hook.FishFlockCohesion &&
                   FishFlockCohesionMaximumDistance == hook.FishFlockCohesionMaximumDistance &&
                   FishFlockSeparation == hook.FishFlockSeparation &&
                   FishFlockSeparationMinimumDistance == hook.FishFlockSeparationMinimumDistance &&
                   FishFlockWeight == hook.FishFlockWeight &&
                   FishOOBWeight == hook.FishOOBWeight &&
                   FishCycle == hook.FishCycle &&
                   FlockAlignment == hook.FlockAlignment &&
                   FlockCohesion == hook.FlockCohesion &&
                   FlockCohesionMaximumDistance == hook.FlockCohesionMaximumDistance &&
                   FlockSeparation == hook.FlockSeparation &&
                   FlockSeparationMinimumDistance == hook.FlockSeparationMinimumDistance &&
                   FlockWeight == hook.FlockWeight &&
                   SnakeWeight == hook.SnakeWeight &&
                   BossMode == hook.BossMode &&
                   EqualityComparer<Sprites[]>.Default.Equals(BossModeSprites, hook.BossModeSprites) &&
                   FlockSpeed == hook.FlockSpeed &&
                   PickupSeekers == hook.PickupSeekers &&
                   PickupRobotGuns == hook.PickupRobotGuns &&
                   PickupSeekersMultiplier == hook.PickupSeekersMultiplier &&
                   SeekerNegotiation == hook.SeekerNegotiation &&
                   SeekerLead == hook.SeekerLead &&
                   SeekerRange == hook.SeekerRange &&
                   SeekerCycle == hook.SeekerCycle &&
                   ShipGainBySizeM == hook.ShipGainBySizeM &&
                   ShipGainBySizeB == hook.ShipGainBySizeB &&
                   MapEnabled == hook.MapEnabled &&
                   StepTime == hook.StepTime &&
                   OutOfBoundsDeathLine == hook.OutOfBoundsDeathLine &&
                   OutOfBoundsBorder == hook.OutOfBoundsBorder &&
                   OutOfBoundsDecayDistance == hook.OutOfBoundsDecayDistance &&
                   BotRespawnDelay == hook.BotRespawnDelay &&
                   BotMaxRespawnDelay == hook.BotMaxRespawnDelay &&
                   PickupShields == hook.PickupShields &&
                   ShieldStrength == hook.ShieldStrength &&
                   FollowFirstShip == hook.FollowFirstShip &&
                   FiringSequenceDelay == hook.FiringSequenceDelay &&
                   SpawnLocationMode == hook.SpawnLocationMode &&
                   SpawnLocation.Equals(hook.SpawnLocation) &&
                   LifecycleDuration == hook.LifecycleDuration &&
                   PlayerCountGracePeriodMS == hook.PlayerCountGracePeriodMS &&
                   FleetWeaponStackDepth == hook.FleetWeaponStackDepth &&
                   SpawnInvulnerabilityTime == hook.SpawnInvulnerabilityTime &&
                   Name == hook.Name &&
                   Description == hook.Description &&
                   Instructions == hook.Instructions &&
                   Hidden == hook.Hidden &&
                   EqualityComparer<string[]>.Default.Equals(AllowedColors, hook.AllowedColors) &&
                   Weight == hook.Weight &&
                   MaxNameLength == hook.MaxNameLength &&
                   GearheadName == hook.GearheadName &&
                   GearheadRegen == hook.GearheadRegen &&
                   AutoRemoveOnEmptyThreshold == hook.AutoRemoveOnEmptyThreshold &&
                   ExplosionTime == hook.ExplosionTime &&
                   AbandonBuffer == hook.AbandonBuffer &&
                   EarnedShipDelay == hook.EarnedShipDelay &&
                   RoyaleCountdownDurationSeconds == hook.RoyaleCountdownDurationSeconds &&
                   RoyaleResizeSpeed == hook.RoyaleResizeSpeed &&
                   RoyaleDoubleStep1 == hook.RoyaleDoubleStep1 &&
                   RoyaleDoubleStep2 == hook.RoyaleDoubleStep2 &&
                   CanSpawn == hook.CanSpawn &&
                   EinsteinCoefficient == hook.EinsteinCoefficient &&
                   Tokens == hook.Tokens;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(WorldSize);
            hash.Add(WorldResizeEnabled);
            hash.Add(WorldResizeSpeed);
            hash.Add(WorldSizeBasic);
            hash.Add(WorldSizeDeltaPerPlayer);
            hash.Add(WorldMinPlayersToResize);
            hash.Add(BaseThrustM);
            hash.Add(BaseThrustB);
            hash.Add(BoostThrust);
            hash.Add(BoostCooldownTimeM);
            hash.Add(BoostCooldownTimeB);
            hash.Add(BoostDuration);
            hash.Add(BoostSpeed);
            hash.Add(Drag);
            hash.Add(BoomLife);
            hash.Add(BoomDrag);
            hash.Add(PointsPerKillShip);
            hash.Add(PointsPerUniverseDeath);
            hash.Add(PointsMultiplierDeath);
            hash.Add(PointsPerKillFleet);
            hash.Add(PointsPerKillFleetMax);
            hash.Add(PointsPerKillFleetStep);
            hash.Add(PointsPerKillFleetPerStep);
            hash.Add(ComboDelay);
            hash.Add(ComboPointsStep);
            hash.Add(HealthHitCost);
            hash.Add(HealthRegenerationPerFrame);
            hash.Add(SpawnShipCount);
            hash.Add(ShotCooldownTimeM);
            hash.Add(ShotCooldownTimeB);
            hash.Add(ShotCooldownTimeShark);
            hash.Add(ShotCooldownTimeBotM);
            hash.Add(ShotCooldownTimeBotB);
            hash.Add(ShotThrustM);
            hash.Add(ShotThrustB);
            hash.Add(MaxHealth);
            hash.Add(MaxHealthBot);
            hash.Add(SeekerThrustMultiplier);
            hash.Add(PrecisionBulletsNoise);
            hash.Add(PrecisionBullets);
            hash.Add(PrecisionBulletsMinimumRange);
            hash.Add(ShieldCannonballLife);
            hash.Add(BulletLife);
            hash.Add(SeekerLifeMultiplier);
            hash.Add(BotBase);
            hash.Add(BotPerXPoints);
            hash.Add(Wormholes);
            hash.Add(WormholesDestination);
            hash.Add(Obstacles);
            hash.Add(PickupShieldsMultiplier);
            hash.Add(ObstaclesMultiplier);
            hash.Add(ObstacleMaxMomentum);
            hash.Add(ObstacleMinSize);
            hash.Add(ObstacleMaxSize);
            hash.Add(ObstacleMaxMomentumWeatherMultiplier);
            hash.Add(ObstaclesSpawnShieldCannons);
            hash.Add(ObstacleBorderBuffer);
            hash.Add(TeamMode);
            hash.Add(RoyaleMode);
            hash.Add(CTFMode);
            hash.Add(CTFCarryBurden);
            hash.Add(CTFSpawnDistance);
            hash.Add(SumoMode);
            hash.Add(SumoRingSize);
            hash.Add(LeaderboardRefresh);
            hash.Add(Fishes);
            hash.Add(FishesMultiplier);
            hash.Add(FishThrust);
            hash.Add(FishFlockAlignment);
            hash.Add(FishFlockCohesion);
            hash.Add(FishFlockCohesionMaximumDistance);
            hash.Add(FishFlockSeparation);
            hash.Add(FishFlockSeparationMinimumDistance);
            hash.Add(FishFlockWeight);
            hash.Add(FishOOBWeight);
            hash.Add(FishCycle);
            hash.Add(FlockAlignment);
            hash.Add(FlockCohesion);
            hash.Add(FlockCohesionMaximumDistance);
            hash.Add(FlockSeparation);
            hash.Add(FlockSeparationMinimumDistance);
            hash.Add(FlockWeight);
            hash.Add(SnakeWeight);
            hash.Add(BossMode);
            hash.Add(BossModeSprites);
            hash.Add(FlockSpeed);
            hash.Add(PickupSeekers);
            hash.Add(PickupRobotGuns);
            hash.Add(PickupSeekersMultiplier);
            hash.Add(SeekerNegotiation);
            hash.Add(SeekerLead);
            hash.Add(SeekerRange);
            hash.Add(SeekerCycle);
            hash.Add(ShipGainBySizeM);
            hash.Add(ShipGainBySizeB);
            hash.Add(MapEnabled);
            hash.Add(StepTime);
            hash.Add(OutOfBoundsDeathLine);
            hash.Add(OutOfBoundsBorder);
            hash.Add(OutOfBoundsDecayDistance);
            hash.Add(BotRespawnDelay);
            hash.Add(BotMaxRespawnDelay);
            hash.Add(PickupShields);
            hash.Add(ShieldStrength);
            hash.Add(FollowFirstShip);
            hash.Add(FiringSequenceDelay);
            hash.Add(SpawnLocationMode);
            hash.Add(SpawnLocation);
            hash.Add(LifecycleDuration);
            hash.Add(PlayerCountGracePeriodMS);
            hash.Add(FleetWeaponStackDepth);
            hash.Add(SpawnInvulnerabilityTime);
            hash.Add(Name);
            hash.Add(Description);
            hash.Add(Instructions);
            hash.Add(Hidden);
            hash.Add(AllowedColors);
            hash.Add(Weight);
            hash.Add(MaxNameLength);
            hash.Add(GearheadName);
            hash.Add(GearheadRegen);
            hash.Add(AutoRemoveOnEmptyThreshold);
            hash.Add(ExplosionTime);
            hash.Add(AbandonBuffer);
            hash.Add(EarnedShipDelay);
            hash.Add(RoyaleCountdownDurationSeconds);
            hash.Add(RoyaleResizeSpeed);
            hash.Add(RoyaleDoubleStep1);
            hash.Add(RoyaleDoubleStep2);
            hash.Add(CanSpawn);
            hash.Add(EinsteinCoefficient);
            hash.Add(Tokens);
            return hash.ToHashCode();
        }
    }
}
