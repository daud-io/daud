namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Steering;
    using Game.Engine.Core.Weapons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class Fleet : ActorGroup
    {
        public virtual float ShotCooldownTimeM { get => World.Hook.ShotCooldownTimeM; }
        public virtual float ShotCooldownTimeB { get => World.Hook.ShotCooldownTimeB; }
        public virtual float ShotThrustM { get => World.Hook.ShotThrustM; }
        public virtual float ShotThrustB { get => World.Hook.ShotThrustB; }
        public virtual float BaseThrustM { get => World.Hook.BaseThrustM; }
        public virtual float BaseThrustB { get => World.Hook.BaseThrustB; }
        public virtual float BoostThrust { get => World.Hook.BoostThrust; }

        public virtual int SpawnShipCount { get => World.Hook.SpawnShipCount; }


        public Player Owner { get; set; }

        public bool BoostRequested { get; set; }
        public bool ShootRequested { get; set; }

        public long ShootCooldownTimeStart { get; set; } = 0;
        public long ShootCooldownTime { get; set; } = 0;
        public float ShootCooldownStatus { get; set; } = 0;
        public long BoostCooldownTimeStart { get; set; } = 0;
        public long BoostCooldownTime { get; set; } = 0;
        public float BoostCooldownStatus { get; set; } = 0;
        public long BoostUntil { get; set; } = 0;

        public Vector2 AimTarget { get; set; }

        public List<Ship> Ships { get; set; } = new List<Ship>();

        public List<ShipWeaponBullet> NewBullets { get; set; } = new List<ShipWeaponBullet>();

        public IFleetWeapon BaseWeapon { get; set; }
        public Stack<IFleetWeapon> WeaponStack { get; set; } = new Stack<IFleetWeapon>();

        public Vector2 FleetCenter = Vector2.Zero;
        public Vector2 FleetVelocity = Vector2.Zero;

        public float Burden { get; set; } = 0f;
        public uint WeaponFiredCount { get; private set; } = 0;

        public Vector2? SpawnLocation { get; set; } = null;
        public int ShipSize { get; set; } = 70;

        public Queue<long> EarnedShips = new Queue<long>();

        [Flags]
        public enum ShipModeEnum
        {
            none = 0,
            boost = 1,
            invulnerable = 2,
            defense_upgrade = 4,
            offense_upgrade = 8,
            shield = 16
        }

        public Sprites BulletSprite
        {
            get
            {
                switch (Owner.ShipSprite)
                {
                    case Sprites.ship_cyan: return Sprites.bullet_cyan;
                    case Sprites.ship_blue: return Sprites.bullet_blue;
                    case Sprites.ship_green: return Sprites.bullet_green;
                    case Sprites.ship_orange: return Sprites.bullet_orange;
                    case Sprites.ship_pink: return Sprites.bullet_pink;
                    case Sprites.ship_red: return Sprites.bullet_red;
                    case Sprites.ship_yellow: return Sprites.bullet_yellow;
                    case Sprites.ship_zed: return Sprites.bullet_red;
                    default: return Sprites.bullet;
                }
            }
        }

        private bool HasOffensiveUpgrade = false;
        private bool HasDefensiveUpgrade = false;

        public Fleet(World world, Player owner):base(world)
        {
            this.Owner = owner;
            this.GroupType = GroupTypes.Fleet;
            this.ZIndex = 100;

            this.BaseWeapon = new FleetWeaponGeneric<ShipWeaponBullet>(World);

            if (SpawnLocation != null)
                FleetCenter = SpawnLocation.Value;
            else
                FleetCenter = world.ChooseSpawnPoint("fleet", this);

            for (int i = 0; i < SpawnShipCount; i++)
                this.AddShip();

        }

        public void Die(Player player)
        {
            World.Scoring.FleetDied(player, Owner, this);
            this.Owner.Die(player);

            PendingDestruction = true;
        }

        public override void Destroy()
        {
            foreach (var ship in Ships.ToList())
                ship.Destroy();

            base.Destroy();
        }

        public void ShipDeath(Player player, Ship ship, ShipWeaponBullet bullet)
        {
            if (!Ships.Where(s => s.Exists && !s.PendingDestruction).Any())
                Die(player);
        }

        public void KilledShip(Ship killedShip)
        {
            var threshold = Ships.Count * World.Hook.ShipGainBySizeM + World.Hook.ShipGainBySizeB;
            if (World.Random.NextDouble() < threshold)
                if (Ships?.Any() ?? false)
                {
                    EarnedShips.Enqueue(World.Time + World.Hook.EarnedShipDelay +1);
                }
        }

        public void AddShip(Sprites? sprite = null)
        {
            if (!this.Owner.IsAlive || this.PendingDestruction)
                return;
                
            Ships.Add(new Ship(World)
            {
                Fleet = this,
                Sprite = this.Owner.ShipSprite,
                Color = this.Owner.Color,
                Size = ShipSize,
                Position = this.FleetCenter + new Vector2
                (
                    World.Random.Next(-20, 20),
                    World.Random.Next(-20, 20)
                ),
                LinearVelocity = FleetVelocity
            });
        }

        public void Abandon()
        {
            foreach (var ship in Ships.ToList())
                this.AbandonShip(ship);
        }

        public void AbandonShip(Ship ship)
        {
            ship.Fleet = null;
            ship.Sprite = Sprites.ship_gray;
            ship.Color = "gray";
            ship.Abandoned = true;
            ship.Group = null;
            ship.ThrustAmount = 0;
            ship.Mode = 0;
            ship.AbandonedByFleet = this;
            ship.AbandonedTime = World.Time;
            ship.ShieldStrength = 0;

            if (Ships.Contains(ship))
                Ships.Remove(ship);
        }


        public void PushStackWeapon(IFleetWeapon weapon)
        {
            WeaponStack.Push(weapon);
            if (WeaponStack.Count > World.Hook.FleetWeaponStackDepth)
                WeaponStack = new Stack<IFleetWeapon>(WeaponStack.TakeLast(World.Hook.FleetWeaponStackDepth));

            this.HasOffensiveUpgrade = WeaponStack.Any(w => w.IsOffense);
            this.HasDefensiveUpgrade = WeaponStack.Any(w => w.IsDefense);
        }

        public override void Think(float dt)
        {
            base.Think(dt);

            var isShooting = ShootRequested && (World.Time >= ShootCooldownTime);
            var isBoosting = World.Time < BoostUntil;
            var isBoostInitial = false;

            if (World.Time > BoostCooldownTime && BoostRequested && Ships.Count > 1)
            {
                BoostCooldownTime = World.Time + (long)(World.Hook.BoostCooldownTimeM * Ships.Count + World.Hook.BoostCooldownTimeB);
                BoostCooldownTimeStart = World.Time;

                BoostUntil = World.Time + World.Hook.BoostDuration;
                isBoostInitial = true;
                var shipLoss = (int)MathF.Floor(Ships.Count / 2);
                var Sorter = Ships.OrderByDescending((ship) => Vector2.DistanceSquared(FleetCenter + AimTarget, ship.Position)).Take(shipLoss);
                foreach (Ship ship in Sorter)
                {
                    AbandonShip(ship);
                }
            }

            FleetCenter = FleetMath.FleetCenterNaive(this.Ships);
            FleetVelocity = FleetMath.FleetVelocity(this.Ships);

            bool killedTooBig = false;
            
            if (!Ships.Where(s => s.Exists && !s.PendingDestruction).Any())
                Die(null);
            else
            {
                var ships = Ships.ToArray();
                foreach (var ship in ships)
                {
                    var shipTargetVector = FleetCenter + AimTarget - ship.Position;

                    ship.Angle = MathF.Atan2(shipTargetVector.Y, shipTargetVector.X);
                    ship.AngularVelocity = 0;

                    Flocking.Flock(ship, ships);

                    ship.ThrustAmount = isBoosting
                        ? BoostThrust * (1 - Burden)
                        : (BaseThrustM * Ships.Count + BaseThrustB) * (1 - Burden);

                    ship.Drag = isBoosting
                        ? 0f
                        : World.Hook.Drag;

                    ship.Mode = (byte)
                        (
                            (isBoosting ? ShipModeEnum.boost : ShipModeEnum.none)
                            | (HasOffensiveUpgrade ? ShipModeEnum.offense_upgrade : ShipModeEnum.none)
                            | (HasDefensiveUpgrade ? ShipModeEnum.defense_upgrade : ShipModeEnum.none)
                            | (Owner.IsInvulnerable ? ShipModeEnum.invulnerable : ShipModeEnum.none)
                            | (ship.ShieldStrength > 0 ? ShipModeEnum.shield : ShipModeEnum.none)
                        );

                    if (isBoostInitial)
                        if (ship.LinearVelocity != Vector2.Zero)
                            ship.LinearVelocity += Vector2.Normalize(ship.LinearVelocity) * World.Hook.BoostSpeed;

                    if (!killedTooBig && Vector2.Distance(ship.Position, FleetCenter) > 1500)
                    {
                        killedTooBig = true; // only do it once per tick
                        AbandonShip(ship);
                    }
                }

                if (isShooting)
                {
                    var weapon = this.WeaponStack.Any()
                        ? this.WeaponStack.Pop()
                        : this.BaseWeapon;

                    weapon.FireFrom(this);
                    this.WeaponFiredCount++;
                    this.HasOffensiveUpgrade = WeaponStack.Any(w => w.IsOffense);
                    this.HasDefensiveUpgrade = WeaponStack.Any(w => w.IsDefense);

                    ShootCooldownTime = World.Time + (int)(ShotCooldownTimeM * Ships.Count + ShotCooldownTimeB);
                    ShootCooldownTimeStart = World.Time;
                }

                if (World.Time > BoostCooldownTime)
                    BoostCooldownStatus = 1;
                else
                    BoostCooldownStatus = (float)
                        (World.Time - BoostCooldownTimeStart) / (BoostCooldownTime - BoostCooldownTimeStart);

                if (World.Time > ShootCooldownTime)
                    ShootCooldownStatus = 1;
                else
                    ShootCooldownStatus = (float)
                        (World.Time - ShootCooldownTimeStart) / (ShootCooldownTime - ShootCooldownTimeStart);

                while (EarnedShips.Any() && EarnedShips.Peek() < World.Time)
                {
                    AddShip();
                    EarnedShips.Dequeue();
                }
            }
        }

        public void ActivateShields()
        {
            foreach (var ship in this.Ships)
            {
                ship.ShieldExpiration = World.Time + World.Hook.ShieldTimeMS;
                ship.ShieldStrength = this.World.Hook.ShieldStrength;
            }
        }
    }
}