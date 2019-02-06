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
        public virtual int ShotCooldownTimeShark { get => World.Hook.ShotCooldownTimeShark; }
        public virtual float ShotThrustM { get => World.Hook.ShotThrustM; }
        public virtual float ShotThrustB { get => World.Hook.ShotThrustB; }
        public virtual float BaseThrustM { get => World.Hook.BaseThrustM; }
        public virtual float BaseThrustB { get => World.Hook.BaseThrustB; }
        public virtual float BoostThrust { get => World.Hook.BoostThrust; }

        public virtual int SpawnShipCount { get => World.Hook.SpawnShipCount; }

        public virtual bool BossMode { get => World.Hook.BossMode && World.Hook.BossModeSprites.Contains(this.Owner.ShipSprite); }

        public Player Owner { get; set; }

        public bool BoostRequested { get; set; }
        public bool ShootRequested { get; set; }

		public long LastKillTime { get; set; } = 0;
		public int KillCounter { get; set; } = 0;
		public int ComboCounter { get; set; } = 0;
        public long ShootCooldownTimeStart { get; set; } = 0;
        public long ShootCooldownTime { get; set; } = 0;
        public float ShootCooldownStatus { get; set; } = 0;
        public long BoostCooldownTimeStart { get; set; } = 0;
        public long BoostCooldownTime { get; set; } = 0;
        public float BoostCooldownStatus { get; set; } = 0;
        public long BoostUntil { get; set; } = 0;

        public Vector2 AimTarget { get; set; }

        public List<Ship> Ships { get; set; } = new List<Ship>();
        public List<Ship> NewShips { get; set; } = new List<Ship>();

        public List<ShipWeaponBullet> NewBullets { get; set; } = new List<ShipWeaponBullet>();

        public IFleetWeapon BaseWeapon { get; set; }
        public Stack<IFleetWeapon> WeaponStack { get; set; } = new Stack<IFleetWeapon>();

        public Vector2 FleetCenter = Vector2.Zero;
        public Vector2 FleetMomentum = Vector2.Zero;

        public float Burden { get; set; } = 0f;
        public bool Shark { get; set; } = false;
        public bool LastTouchedLeft { get; set; } = false;
        public bool FiringWeapon { get; private set; } = false;

        public string CustomData { get; set; }

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

        private void Die(Player player)
        {
            if (player != null)
            {
				var comboTxt = "";
				var comboPlusScore = 0;
				if (player.IsAlive)
                {
					player.Fleet.KillCounter += 1;
                    if (World.Time - player.Fleet.LastKillTime < World.Hook.ComboDelay)
                    {
                        player.Fleet.ComboCounter += 1;
                        comboTxt = $"x{player.Fleet.ComboCounter} combo!";
                        comboPlusScore = (player.Fleet.ComboCounter - 1) * World.Hook.ComboPointsStep;
                        player.Score += comboPlusScore;
                    }
                    else
                    {
                        player.Fleet.ComboCounter = 1;
                    }

                    var PreviousKillTime = player.Fleet.LastKillTime;
                    player.Fleet.LastKillTime = World.Time;

                    int plusScore = Convert.ToInt32(World.Hook.PointsPerKillFleetStep * (Math.Floor((decimal)this.Owner.Score / (decimal)World.Hook.PointsPerKillFleetPerStep) + 1));
                    plusScore = (plusScore < World.Hook.PointsPerKillFleetMax) ? plusScore : World.Hook.PointsPerKillFleetMax;
                    player.Score += plusScore;

                    player.SendMessage($"You Killed {this.Owner.Name}", "kill",
                        plusScore,
                        new
                        {
                            ping = new
                            {
                                you = player?.Connection?.Latency ?? 0,
                                them = this.Owner?.Connection?.Latency ?? 0
                            },
                            combo = new
                            {
                                text = comboTxt,
                                score = comboPlusScore
                            }
                        }
                    );
                }
                //player.SendMessage($"You Killed {this.Owner.Name}! - +{plusScore}{combo} - ping (you: {player?.Connection?.Latency ?? 0} them:{this.Owner?.Connection?.Latency ?? 0})");
                if (this.Owner.Connection != null)
                    this.Owner.Connection.SpectatingFleet = player.Fleet;
					//this.Owner.SendMessage($"Killed by {player.Name} - ping (you: {this.Owner?.Connection?.Latency ?? 0} them:{player?.Connection?.Latency ?? 0})");
					this.Owner.SendMessage($"Killed by {player.Name}", "killed",
						(int)MathF.Ceiling(this.Owner.Score / 2),
						new
						{
							score = this.Owner.Score,
							kills = player.Fleet.KillCounter,
							ping = new
							{
								you = this.Owner?.Connection?.Latency ?? 0,
								them = player?.Connection?.Latency ?? 0
							}
						}
					);
					player.Fleet.KillCounter = 0;
            }
            else
            {
                if (this.Owner != null)
                {
                    this.Owner.SendMessage($"Killed by the universe", pointsDelta: World.Hook.PointsPerUniverseDeath);
                    this.Owner.Score += World.Hook.PointsPerUniverseDeath;
                }
            }

            this.Owner.Die(player);

            PendingDestruction = true;
            NewShips.Clear();
        }

        public override void Destroy()
        {
            foreach (var ship in Ships.ToList())
                ship.Destroy();

            base.Destroy();
        }

        public void ShipDeath(Player player, Ship ship, ShipWeaponBullet bullet)
        {
            if (!Ships.Where(s => !s.PendingDestruction).Any())
                Die(player);
        }

        public void AddShip()
        {
            if (!this.Owner.IsAlive || this.PendingDestruction)
                return;

            var random = new Random();

            var offset = new Vector2
            (
                random.Next(-20, 20),
                random.Next(-20, 20)
            );

            var ship = new Ship()
            {
                Fleet = this,
                Sprite = this.Owner.ShipSprite,
                Color = this.Owner.Color
            };

            if (this.Ships.Any())
            {
                var position = Vector2.Zero;
                var momentum = Vector2.Zero;
                var angle = 0f;
                var count = 0;

                foreach (var existingShip in this.Ships)
                {
                    position += existingShip.Position;
                    momentum += existingShip.Momentum;
                    angle += existingShip.Angle;
                    count++;
                }

                ship.Position = position / count + offset;
                ship.Momentum = momentum / count;
                ship.Angle = angle / count;
            }
            else
            {
                ship.Position = FleetCenter + offset;
            }

            NewShips.Add(ship);
        }

        public override void Init(World world)
        {
            base.Init(world);
            this.GroupType = GroupTypes.Fleet;
            this.ZIndex = 100;

            this.BaseWeapon = new FleetWeaponGeneric<ShipWeaponBullet>();

            FleetCenter = world.RandomSpawnPosition(this);

            for (int i = 0; i < SpawnShipCount; i++)
                this.AddShip();
        }

        public override void CreateDestroy()
        {
            /*if (this.Owner != null && this.Owner.IsAlive)
                this.PendingDestruction = true;*/

            if (FiringWeapon)
            {

                var weapon = this.WeaponStack.Any()
                    ? this.WeaponStack.Pop()
                    : this.BaseWeapon;

                weapon.FireFrom(this);
                FiringWeapon = false;
            }

            foreach (var ship in NewShips)
            {
                ship.Init(World);
                Ships.Add(ship);
            }
            NewShips.Clear();

            foreach (var bullet in NewBullets)
                bullet.Init(World);

            NewBullets.Clear();

            base.CreateDestroy();
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

            if (Ships.Contains(ship))
                Ships.Remove(ship);
        }


        public void PushStackWeapon(IFleetWeapon weapon)
        {
            WeaponStack.Push(weapon);
            if (WeaponStack.Count > World.Hook.FleetWeaponStackDepth)
                WeaponStack = new Stack<IFleetWeapon>(WeaponStack.TakeLast(World.Hook.FleetWeaponStackDepth));
        }

        public override void Think()
        {
            var isShooting = ShootRequested && World.Time >= ShootCooldownTime;
            var isBoosting = World.Time < BoostUntil;
            var isBoostInitial = false;

            if (World.Time > BoostCooldownTime && BoostRequested && Ships.Count > 1)
            {
                BoostCooldownTime = World.Time + (long)
                    (World.Hook.BoostCooldownTimeM * Ships.Count + World.Hook.BoostCooldownTimeB);
                BoostCooldownTimeStart = World.Time;

                BoostUntil = World.Time + World.Hook.BoostDuration;
                isBoostInitial = true;
                var shipLoss = (int)MathF.Floor(Ships.Count / 2);
                for (int i = 0; i < shipLoss; i++)
                {
                    var ship = Ships.First();
                    AbandonShip(ship);
                }
            }

            FleetCenter = Flocking.FleetCenterNaive(this.Ships);
            FleetMomentum = Flocking.FleetMomentum(this.Ships);

            var summation = new Vector2();
            var moment = new Vector2();
            foreach (var ship in Ships)
            {
                var shipTargetVector = FleetCenter + AimTarget - ship.Position;

                ship.Angle = MathF.Atan2(shipTargetVector.Y, shipTargetVector.X);

                if(Ships.IndexOf(ship)<5){
                    summation+=ship.Position;
                    moment += ship.Momentum;
                }
                Flock(ship);
                Snake(ship);
                Ring(ship, summation, moment);

                ship.ThrustAmount = isBoosting
                    ? BoostThrust * (1 - Burden)
                    : (BaseThrustM * Ships.Count + BaseThrustB) * (1 - Burden);

                ship.Drag = isBoosting
                    ? 1.0f
                    : World.Hook.Drag;

                ship.Mode = (byte)
                    (
                        (isBoosting ? ShipModeEnum.boost : ShipModeEnum.none)
                        | (WeaponStack.Any(w => w.IsOffense) ? ShipModeEnum.offense_upgrade : ShipModeEnum.none)
                        | (WeaponStack.Any(w => w.IsDefense) ? ShipModeEnum.defense_upgrade : ShipModeEnum.none)
                        | (!Owner.IsShielded && Owner.IsInvulnerable ? ShipModeEnum.invulnerable : ShipModeEnum.none)
                        | (Owner.IsShielded ? ShipModeEnum.shield : ShipModeEnum.none)
                    );

                if (isBoostInitial)
                    if (ship.Momentum != Vector2.Zero)
                        ship.Momentum += Vector2.Normalize(ship.Momentum) * World.Hook.BoostSpeed;
            }

            if (isShooting)
            {
                ShootCooldownTime = World.Time + (Shark ? ShotCooldownTimeShark : (int)(ShotCooldownTimeM * Ships.Count + ShotCooldownTimeB));
                ShootCooldownTimeStart = World.Time;

                FiringWeapon = true;
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

            if (MathF.Abs(FleetCenter.X) >  World.Hook.WorldSize &&
                FleetCenter.X<0!=LastTouchedLeft &&
                World.Hook.Name == "Sharks and Minnows" &&
                !Owner.IsInvulnerable &&
                !Shark)
            {
                LastTouchedLeft = FleetCenter.X < 0;
                Owner.IsInvulnerable = true;
                Owner.SpawnTime = World.Time;
                Owner.Score++;
            }

            if (!Ships.Where(s => !s.PendingDestruction).Any()
                && !NewShips.Any())
                Die(null);
        }

        private void Flock(Ship ship)
        {
            if (World.Hook.FlockWeight == 0)
                return;

            if (Ships.Count < 2)
                return;

            var shipFlockingVector =
                (World.Hook.FlockCohesion * Flocking.Cohesion(Ships, ship, World.Hook.FlockCohesionMaximumDistance))
                + (World.Hook.FlockSeparation * Flocking.Separation(Ships, ship, World.Hook.FlockSeparationMinimumDistance));

            var steeringVector = new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle));

            steeringVector += World.Hook.FlockWeight * shipFlockingVector;

            ship.Angle = MathF.Atan2(steeringVector.Y, steeringVector.X);
        }

        private void Ring(Ship ship, Vector2 average, Vector2 momentum)
        {
            if (!BossMode)
                return;

            var targetAngle = MathF.Atan2(AimTarget.Y, AimTarget.X);
            var shipIndex = Ships.IndexOf(ship);
            var innerAngle = (shipIndex - 5) / (float)(Ships.Count - 5) * 2 * MathF.PI;
            var angle = (shipIndex - 5) / (float)(Ships.Count - 5) * 2 * MathF.PI;
            if (shipIndex > 4)
            {
                ship.Position = average/5 + 
                    new Vector2(
                        MathF.Cos(angle + targetAngle), 
                        MathF.Sin(angle + targetAngle)
                    ) * (50 + 15 * Ships.Count);
                ship.Momentum = momentum / 5;
                ship.Angle = angle + targetAngle;

            }
        }
        private void Snake(Ship ship)
        {
            if (World.Hook.SnakeWeight == 0)
                return;
            if (Ships.Count < 2)
                return;

            var shipIndex = Ships.IndexOf(ship);
            if (shipIndex > 0)
            {
                ship.Size = 70;
                var steeringVector = new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle));
                steeringVector += (Ships[shipIndex - 1].Position - ship.Position) * World.Hook.SnakeWeight;
                ship.Angle = MathF.Atan2(steeringVector.Y, steeringVector.X);
            }
            else
                ship.Size = 100;
        }
    }
}