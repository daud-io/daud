namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Steering;
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
        public List<Ship> NewShips { get; set; } = new List<Ship>();

        public List<Bullet> NewBullets { get; set; } = new List<Bullet>();

        public Pickup Pickup = null;

        public Vector2 FleetCenter = Vector2.Zero;
        public Vector2 FleetMomentum = Vector2.Zero;

        public float Burden { get; set; } = 0f;
        private bool FireVolley = false;

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
                player.Score += World.Hook.PointsPerKillFleet;

                player.SendMessage($"You Killed {this.Owner.Name}");
                if (this.Owner.Connection != null)
                    this.Owner.Connection.SpectatingFleet = player.Fleet;
                this.Owner.SendMessage($"Killed by {player.Name}");
            }
            else
            {
                if (this.Owner != null)
                {
                    this.Owner.SendMessage($"Killed by the universe");
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

        public void ShipDeath(Player player, Ship ship, Bullet bullet)
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

            FleetCenter = world.RandomSpawnPosition(this);

            for (int i = 0; i < world.Hook.SpawnShipCount; i++)
                this.AddShip();
        }

        public override void CreateDestroy()
        {
            /*if (this.Owner != null && this.Owner.IsAlive)
                this.PendingDestruction = true;*/

            if (FireVolley)
            {
                Volley.FireFrom(this);
                FireVolley = false;
                Pickup = null;
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
            foreach (var ship in Ships)
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
                    Ships.Remove(ship);
                }
            }

            FleetCenter = Flocking.FleetCenterNaive(this.Ships);
            FleetMomentum = Flocking.FleetMomentum(this.Ships);

            foreach (var ship in Ships)
            {
                var shipTargetVector = FleetCenter + AimTarget - ship.Position;

                ship.Angle = MathF.Atan2(shipTargetVector.Y, shipTargetVector.X);

                Flock(ship);

                ship.ThrustAmount = isBoosting
                    ? BoostThrust * (1-Burden)
                    : (BaseThrustM * Ships.Count + BaseThrustB) * (1 - Burden);

                ship.Drag = isBoosting
                    ? 1.0f
                    : World.Hook.Drag;

                ship.Mode = (byte)(isBoosting
                    ? 1
                    : 0);

                if (isBoostInitial)
                    if (ship.Momentum != Vector2.Zero)
                        ship.Momentum += Vector2.Normalize(ship.Momentum) * World.Hook.BoostSpeed;
            }

            if (isShooting)
            {
                ShootCooldownTime = World.Time + (int)(ShotCooldownTimeM * Ships.Count + ShotCooldownTimeB);
                ShootCooldownTimeStart = World.Time;

                /*foreach (var ship in Ships)
                    NewBullets.Add(Bullet.FireFrom(ship));*/

                FireVolley = true;
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

            if(World.DistanceOutOfBounds(FleetCenter)>0 && World.Name=="Sharks and Minnows"){
                Console.WriteLine("yaaaaaaaaaaaa");
                Owner.IsInvulnerable = true;
                Owner.SpawnTime = World.Time;
            }
        }

        private void Flock(Ship ship)
        {
            if (Ships.Count < 2)
                return;

            var shipFlockingVector =
                (World.Hook.FlockCohesion * Flocking.Cohesion(Ships, ship, World.Hook.FlockCohesionMaximumDistance))
                + (World.Hook.FlockSeparation * Flocking.Separation(Ships, ship, World.Hook.FlockSeparationMinimumDistance));

            var steeringVector = new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle));

            steeringVector += World.Hook.FlockWeight * shipFlockingVector;

            ship.Angle = MathF.Atan2(steeringVector.Y, steeringVector.X);
        }
    }
}