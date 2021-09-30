namespace Game.API.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public class Hook
    {
        public Hook()
        {
            this.AllowedColors = AllColors;
        }

        public static Hook Default
        {
            get
            {
                return new Hook();
            }
        }


        public int WorldSize { get; set; } = 8000;
        public MeshConfiguration Mesh { get; set; } = new MeshConfiguration();
        public class MeshConfiguration
        {
            public bool Enabled { get; set; } = false;
            public string MeshURL { get; set; } = null;
        }

        public float BaseThrustM { get; set; } = -0.0035f;
        public float BaseThrustB { get; set; } = 0.15f;

        public float BoostThrust { get; set; } = 0.15f;

        public float BoostCooldownTimeM { get; set; } = 14.0f;
        public float BoostCooldownTimeB { get; set; } = 1080.0f;

        public int BoostDuration { get; set; } = 420;
        public float BoostSpeed { get; set; } = 1f;

        public float Drag { get; set; } = 0.002f;

        public int BoomLife { get; set; } = 500;

        public int PointsPerKillShip { get; set; } = 1;
        public int PointsPerUniverseDeath { get; set; } = -1;
        public float PointsMultiplierDeath { get; set; } = 0.5f;
        public int PointsPerKillFleet { get; set; } = 55;
        public int PointsPerKillFleetMax { get; set; } = 55;
        public int PointsPerKillFleetStep { get; set; } = 5;
        public float PointsPerKillFleetPerStep { get; set; } = 50;
        public int ComboDelay { get; set; } = 4000;
        public int ComboPointsStep { get; set; } = 5;

        public int SpawnShipCount { get; set; } = 5;

        public float ShotCooldownTimeM { get; set; } = 20;
        public float ShotCooldownTimeB { get; set; } = 550;

        public float ShotThrustM { get; set; } = -0.006f;
        public float ShotThrustB { get; set; } = 0.22f;

        public float SeekerThrustMultiplier { get; set; } = 1.35f;
        public float SeekerLifeMultiplier { get; set; } = 1.15f;
        public bool PrecisionBullets { get; set; } = false;
        public float PrecisionBulletsNoise { get; set; } = 0;
        public float PrecisionBulletsMinimumRange { get; set; } = 0;

        public int BulletLife { get; set; } = 1500;

        public int Obstacles { get; set; } = 10;
        public int ObstacleMinSize { get; set; } = 50;
        public int ObstacleMaxSize { get; set; } = 200;

        public bool TeamMode { get; set; } = false;
        public bool RoyaleMode { get; set; } = false;
        public bool CTFMode { get; set; } = false;
        public float CTFCarryBurden { get; set; } = 0.2f;
        public int CTFSpawnDistance { get; set; } = 6000;
        public bool SumoMode { get; set; } = false;
        public int SumoRingSize { get; set; } = 1000;

        public int LeaderboardRefresh { get; set; } = 750;

        public int Fishes { get; set; } = 35;
        public float FishThrust { get; set; } = 0.08f;
        public float FishFlockAlignment { get; set; } = 5f;
        public float FishFlockCohesion { get; set; } = 0.01f;
        public int FishFlockCohesionMaximumDistance { get; set; } = 1000;
        public float FishFlockSeparation { get; set; } = 50;
        public int FishFlockSeparationMinimumDistance { get; set; } = 200;
        public float FishFlockWeight { get; set; } = 1;
        public int FishCycle { get; set; } = 300;


        public float FlockAlignment { get; set; } = 0.35f;
        public float FlockCohesion { get; set; } = 0.006f;
        public int FlockCohesionMaximumDistance { get; set; } = 600;
        public float FlockSeparation { get; set; } = 80f;
        public int FlockSeparationMinimumDistance { get; set; } = 200;
        public float FlockWeight { get; set; } = 0.14f;
        public int FlockSpeed { get; set; } = 0;

        public int PickupSeekers { get; set; } = 5;

        public bool SeekerNegotiation { get; set; } = true;
        public int SeekerLead { get; set; } = 150;
        public int SeekerRange { get; set; } = 2100;
        public int SeekerCycle { get; set; } = 250;

        public float ShipGainBySizeM { get; set; } = -0.034f;
        public float ShipGainBySizeB { get; set; } = 1.03f;

        public int StepTime { get; set; } = 20;

        public int PickupShields { get; set; } = 3;
        public int ShieldStrength { get; set; } = 3;

        public int FiringSequenceDelay { get; set; } = 0;

        public string SpawnLocationMode { get; set; } = "QuietSpot";
        public Vector2 SpawnLocation { get; set; } = Vector2.Zero;

        public int PlayerCountGracePeriodMS { get; set; } = 15000;
        public int FleetWeaponStackDepth { get; set; } = 1;
        public int ShieldTimeMS { get; set; } = 3000;


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

        public string Name { get; set; } = "FFA";
        public string Description { get; set; } = null;
        public string Instructions { get; set; } = null;

        public bool Hidden { get; set; } = false;
        public string[] AllowedColors { get; set; }

        public int Weight { get; set; } = 0;
        public int MaxNameLength { get; set; } = 17;
        public int AutoRemoveOnEmptyThreshold { get; set; } = 0;
        public int AbandonBuffer { get; set; } = 120;
        public int EarnedShipDelay { get; set; } = 0;
        public int RoyaleCountdownDurationSeconds { get; set; } = 5;
        public int RoyaleResizeSpeed { get; set; } = 4;
        public int RoyaleDoubleStep1 { get; set; } = 4200;
        public int RoyaleDoubleStep2 { get; set; } = 1500;
        public bool CanSpawn { get; set; } = true;
        public float EinsteinCoefficient { get; set; } = 0.25f;
        public int Tokens { get; set; } = 0;

        public Hook Clone()
        {
            return this.MemberwiseClone() as Hook;
        }

        public override bool Equals(object obj)
        {
            return obj is Hook hook &&
                   WorldSize == hook.WorldSize &&
                   BaseThrustM == hook.BaseThrustM &&
                   BaseThrustB == hook.BaseThrustB &&
                   BoostThrust == hook.BoostThrust &&
                   BoostCooldownTimeM == hook.BoostCooldownTimeM &&
                   BoostCooldownTimeB == hook.BoostCooldownTimeB &&
                   BoostDuration == hook.BoostDuration &&
                   BoostSpeed == hook.BoostSpeed &&
                   Drag == hook.Drag &&
                   BoomLife == hook.BoomLife &&
                   PointsPerKillShip == hook.PointsPerKillShip &&
                   PointsPerUniverseDeath == hook.PointsPerUniverseDeath &&
                   PointsMultiplierDeath == hook.PointsMultiplierDeath &&
                   PointsPerKillFleet == hook.PointsPerKillFleet &&
                   PointsPerKillFleetMax == hook.PointsPerKillFleetMax &&
                   PointsPerKillFleetStep == hook.PointsPerKillFleetStep &&
                   PointsPerKillFleetPerStep == hook.PointsPerKillFleetPerStep &&
                   ComboDelay == hook.ComboDelay &&
                   ComboPointsStep == hook.ComboPointsStep &&
                   SpawnShipCount == hook.SpawnShipCount &&
                   ShotCooldownTimeM == hook.ShotCooldownTimeM &&
                   ShotCooldownTimeB == hook.ShotCooldownTimeB &&
                   ShotThrustM == hook.ShotThrustM &&
                   ShotThrustB == hook.ShotThrustB &&
                   SeekerThrustMultiplier == hook.SeekerThrustMultiplier &&
                   PrecisionBulletsNoise == hook.PrecisionBulletsNoise &&
                   PrecisionBullets == hook.PrecisionBullets &&
                   PrecisionBulletsMinimumRange == hook.PrecisionBulletsMinimumRange &&
                   BulletLife == hook.BulletLife &&
                   SeekerLifeMultiplier == hook.SeekerLifeMultiplier &&
                   Obstacles == hook.Obstacles &&
                   ObstacleMinSize == hook.ObstacleMinSize &&
                   ObstacleMaxSize == hook.ObstacleMaxSize &&
                   TeamMode == hook.TeamMode &&
                   RoyaleMode == hook.RoyaleMode &&
                   CTFMode == hook.CTFMode &&
                   CTFCarryBurden == hook.CTFCarryBurden &&
                   CTFSpawnDistance == hook.CTFSpawnDistance &&
                   SumoMode == hook.SumoMode &&
                   SumoRingSize == hook.SumoRingSize &&
                   LeaderboardRefresh == hook.LeaderboardRefresh &&
                   Fishes == hook.Fishes &&
                   FishThrust == hook.FishThrust &&
                   FishFlockAlignment == hook.FishFlockAlignment &&
                   FishFlockCohesion == hook.FishFlockCohesion &&
                   FishFlockCohesionMaximumDistance == hook.FishFlockCohesionMaximumDistance &&
                   FishFlockSeparation == hook.FishFlockSeparation &&
                   FishFlockSeparationMinimumDistance == hook.FishFlockSeparationMinimumDistance &&
                   FishFlockWeight == hook.FishFlockWeight &&
                   FishCycle == hook.FishCycle &&
                   FlockAlignment == hook.FlockAlignment &&
                   FlockCohesion == hook.FlockCohesion &&
                   FlockCohesionMaximumDistance == hook.FlockCohesionMaximumDistance &&
                   FlockSeparation == hook.FlockSeparation &&
                   FlockSeparationMinimumDistance == hook.FlockSeparationMinimumDistance &&
                   FlockWeight == hook.FlockWeight &&
                   FlockSpeed == hook.FlockSpeed &&
                   PickupSeekers == hook.PickupSeekers &&
                   SeekerNegotiation == hook.SeekerNegotiation &&
                   SeekerLead == hook.SeekerLead &&
                   SeekerRange == hook.SeekerRange &&
                   SeekerCycle == hook.SeekerCycle &&
                   ShipGainBySizeM == hook.ShipGainBySizeM &&
                   ShipGainBySizeB == hook.ShipGainBySizeB &&
                   StepTime == hook.StepTime &&
                   PickupShields == hook.PickupShields &&
                   ShieldStrength == hook.ShieldStrength &&
                   FiringSequenceDelay == hook.FiringSequenceDelay &&
                   SpawnLocationMode == hook.SpawnLocationMode &&
                   SpawnLocation.Equals(hook.SpawnLocation) &&
                   PlayerCountGracePeriodMS == hook.PlayerCountGracePeriodMS &&
                   FleetWeaponStackDepth == hook.FleetWeaponStackDepth &&
                   ShieldTimeMS == hook.ShieldTimeMS &&
                   Name == hook.Name &&
                   Description == hook.Description &&
                   Instructions == hook.Instructions &&
                   Hidden == hook.Hidden &&
                   EqualityComparer<string[]>.Default.Equals(AllowedColors, hook.AllowedColors) &&
                   Weight == hook.Weight &&
                   MaxNameLength == hook.MaxNameLength &&
                   AutoRemoveOnEmptyThreshold == hook.AutoRemoveOnEmptyThreshold &&
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
            hash.Add(BaseThrustM);
            hash.Add(BaseThrustB);
            hash.Add(BoostThrust);
            hash.Add(BoostCooldownTimeM);
            hash.Add(BoostCooldownTimeB);
            hash.Add(BoostDuration);
            hash.Add(BoostSpeed);
            hash.Add(Drag);
            hash.Add(BoomLife);
            hash.Add(PointsPerKillShip);
            hash.Add(PointsPerUniverseDeath);
            hash.Add(PointsMultiplierDeath);
            hash.Add(PointsPerKillFleet);
            hash.Add(PointsPerKillFleetMax);
            hash.Add(PointsPerKillFleetStep);
            hash.Add(PointsPerKillFleetPerStep);
            hash.Add(ComboDelay);
            hash.Add(ComboPointsStep);
            hash.Add(SpawnShipCount);
            hash.Add(ShotCooldownTimeM);
            hash.Add(ShotCooldownTimeB);
            hash.Add(ShotThrustM);
            hash.Add(ShotThrustB);
            hash.Add(SeekerThrustMultiplier);
            hash.Add(PrecisionBulletsNoise);
            hash.Add(PrecisionBullets);
            hash.Add(PrecisionBulletsMinimumRange);
            hash.Add(BulletLife);
            hash.Add(SeekerLifeMultiplier);
            hash.Add(Obstacles);
            hash.Add(ObstacleMinSize);
            hash.Add(ObstacleMaxSize);
            hash.Add(TeamMode);
            hash.Add(RoyaleMode);
            hash.Add(CTFMode);
            hash.Add(CTFCarryBurden);
            hash.Add(CTFSpawnDistance);
            hash.Add(SumoMode);
            hash.Add(SumoRingSize);
            hash.Add(LeaderboardRefresh);
            hash.Add(Fishes);
            hash.Add(FishThrust);
            hash.Add(FishFlockAlignment);
            hash.Add(FishFlockCohesion);
            hash.Add(FishFlockCohesionMaximumDistance);
            hash.Add(FishFlockSeparation);
            hash.Add(FishFlockSeparationMinimumDistance);
            hash.Add(FishFlockWeight);
            hash.Add(FishCycle);
            hash.Add(FlockAlignment);
            hash.Add(FlockCohesion);
            hash.Add(FlockCohesionMaximumDistance);
            hash.Add(FlockSeparation);
            hash.Add(FlockSeparationMinimumDistance);
            hash.Add(FlockWeight);
            hash.Add(FlockSpeed);
            hash.Add(PickupSeekers);
            hash.Add(SeekerNegotiation);
            hash.Add(SeekerLead);
            hash.Add(SeekerRange);
            hash.Add(SeekerCycle);
            hash.Add(ShipGainBySizeM);
            hash.Add(ShipGainBySizeB);
            hash.Add(StepTime);
            hash.Add(PickupShields);
            hash.Add(ShieldStrength);
            hash.Add(FiringSequenceDelay);
            hash.Add(SpawnLocationMode);
            hash.Add(SpawnLocation);
            hash.Add(PlayerCountGracePeriodMS);
            hash.Add(FleetWeaponStackDepth);
            hash.Add(ShieldTimeMS);
            hash.Add(Name);
            hash.Add(Description);
            hash.Add(Instructions);
            hash.Add(Hidden);
            hash.Add(AllowedColors);
            hash.Add(Weight);
            hash.Add(MaxNameLength);
            hash.Add(AutoRemoveOnEmptyThreshold);
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
