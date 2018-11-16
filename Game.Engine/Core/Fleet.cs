namespace Game.Engine.Core
{
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

        public long TimeReloaded { get; set; } = 0;
        public long BoostCooldownTime { get; set; } = 0;
        public long BoostUntil { get; set; } = 0;

        public Vector2 AimTarget { get; set; }

        public List<Ship> Ships { get; set; } = new List<Ship>();
        public List<Ship> NewShips { get; set; } = new List<Ship>();

        public List<Bullet> NewBullets { get; set; } = new List<Bullet>();

        public Pickup Pickup = null;
        public Sprites BulletSprite = Sprites.bullet;

        public Vector2 FleetCenter = Vector2.Zero;
        public Vector2 FleetMomentum = Vector2.Zero;

        private void Die(Player player)
        {
            if (player != null)
            {
                player.Score += 55;

                player.SendMessage($"You Killed {this.Owner.Name}");
                this.Owner.SendMessage($"Killed by {player.Name}");
            }

            this.Owner.Die();

            PendingDestruction = true;
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
                Sprite = this.Owner.ShipSprite
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

            FleetCenter = world.RandomPosition();

            for (int i=0; i<world.Hook.SpawnShipCount; i++)
                this.AddShip();
        }

        public override void CreateDestroy()
        {
            base.CreateDestroy();

            foreach (var ship in NewShips)
            {
                ship.Init(World);
                Ships.Add(ship);
            }
            NewShips.Clear();

            foreach (var bullet in NewBullets)
                bullet.Init(World);

            NewBullets.Clear();
        }

        public override void Think()
        {
            var isShooting = ShootRequested && World.Time >= TimeReloaded;
            var isBoosting = World.Time < BoostUntil;
            var isBoostInitial = false;

            if (World.Time > BoostCooldownTime && BoostRequested && Ships.Count > 1)
            {
                BoostCooldownTime = World.Time + World.Hook.BoostCooldownTime;
                BoostUntil = World.Time + World.Hook.BoostDuration;
                isBoostInitial = true;
                var shipLoss = (int)MathF.Floor(Ships.Count / 2);
                for (int i = 0; i < shipLoss; i++)
                {
                    var ship = Ships.First();
                    ship.Fleet = null;
                    ship.Sprite = Sprites.ship_gray;
                    ship.Color = "gray";
                    ship.Abandoned = true;
                    ship.Group = null;
                    ship.ThrustAmount = 0;
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
                    ? BoostThrust
                    : BaseThrustM * Ships.Count + BaseThrustB;

                ship.Drag = isBoosting
                    ? 1.0f
                    : World.Hook.Drag;

                if (isBoostInitial)
                    ship.Momentum += Vector2.Normalize(ship.Momentum) * World.Hook.BoostSpeed;
            }

            if (isShooting)
            {
                TimeReloaded = World.Time + (int)(ShotCooldownTimeM * Ships.Count + ShotCooldownTimeB);

                foreach (var ship in Ships)
                    NewBullets.Add(Bullet.FireFrom(ship));

                this.Pickup = null;
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