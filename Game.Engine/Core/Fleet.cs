namespace Game.Engine.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class Fleet : ActorBody
    {
        public virtual int ShootCooldownTime { get => World.Hook.ShootCooldownTime; }
        public virtual float BaseThrust { get => World.Hook.BaseThrust; }
        public virtual float MaxSpeed { get => World.Hook.MaxSpeed; }
        public virtual float MaxSpeedBoost { get => World.Hook.MaxSpeedBoost; }

        public Player Owner { get; set; }

        public bool BoostRequested { get; set; }
        public bool ShootRequested { get; set; }

        public long TimeReloaded { get; set; } = 0;
        public long BoostCooldownTime { get; set; } = 0;
        public long BoostingUntil { get; set; } = 0;
        public Vector2 BoostVector { get; set; }

        private long NextFlockingTime = 0;

        public List<Ship> Ships { get; set; } = new List<Ship>();

        private void Die(Player player)
        {
            if (player != null)
            {
                player.Score += 55;

                player.SendMessage($"You Killed {this.Owner.Name}");
                this.Owner.SendMessage($"Killed by {player.Name}");
            }

            this.Owner.Die();
            Deinit();
        }

        public override void Deinit()
        {
            foreach (var ship in Ships)
                ship.Deinit();

            base.Deinit();
        }

        public void ShipDeath(Player player, Ship ship, Bullet bullet)
        {
            Ships.Remove(ship);

            if (Ships.Count == 0)
                Die(player);

        }

        public void AddShip()
        {
            if (!this.Owner.IsAlive)
                return;

            var random = new Random();

            var offset = new Vector2
            (
                random.Next(-20, 20),
                random.Next(-20, 20)
            );

            var ship = new Ship()
            {
                Owner = this,
                Position = Vector2.Add(this.Position, offset),
                Momentum = this.Momentum,
                Sprite = this.Owner.ShipSprite,
                Color = this.Color
            };

            ship.Init(World);
            Ships.Add(ship);
        }

        public override void Init(World world)
        {
            this.Position = world.RandomPosition();

            base.Init(world);

            this.AddShip();
            this.AddShip();
            this.AddShip();
            this.AddShip();
            this.AddShip();
            this.AddShip();
            this.AddShip();
        }

        public override void Step()
        {
            var isShooting = ShootRequested && World.Time >= TimeReloaded;
            var isBoosting = World.Time < BoostingUntil;

            if (World.Time > BoostCooldownTime && BoostRequested)
            {
                BoostCooldownTime = World.Time + 1500;
                BoostingUntil = World.Time + 750;
                BoostVector = Vector2.Normalize(Momentum) * 0.02f;
            }

            foreach (var ship in Ships)
            {
                ship.Angle = Angle;

                Flock(ship);

                float thrustAmount = BaseThrust;

                if (isBoosting)
                    thrustAmount *= 1.5f;

                var thrust =
                    Vector2.Transform(
                        new Vector2(thrustAmount, 0),
                        Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), ship.Angle)
                    );

                float speedLimit = isBoosting
                    ? MaxSpeedBoost
                    : MaxSpeed;

                var x = Vector2.Add(ship.Momentum, thrust);
                var currentSpeed = Math.Abs(Vector2.Distance(x, Vector2.Zero));
                if (currentSpeed > speedLimit)
                    x = Vector2.Multiply(Vector2.Normalize(x), ((speedLimit + 3 * currentSpeed) / 4));

                if (isBoosting)
                    x += BoostVector;

                ship.Momentum = x;
                Momentum = x;
            }

            var fleetCenter = FleetCenterNaive(null);
            var cameraVector = fleetCenter - Position;

            this.Momentum += cameraVector * 0.005f;

            if (isShooting)
            {
                TimeReloaded = World.Time + ShootCooldownTime;

                foreach (var ship in Ships)
                    Bullet.FireFrom(ship);
            }
        }

        private void Flock(Ship ship)
        {
            if (Ships.Count < 2)
                return;

            if (World.Time >= NextFlockingTime)
            {
                NextFlockingTime = World.Time + World.Hook.FlockSpeed;

                var shipFlockingVector =
                    (World.Hook.FlockCohesion * Cohesion(ship, World.Hook.FlockCohesionMaximumDistance))
                    + (World.Hook.FlockAlignment * Alignment(ship))
                    + (World.Hook.FlockSeparation * Separation(ship, World.Hook.FlockSeparationMinimumDistance));

                var steeringVector = new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle));

                steeringVector += World.Hook.FlockWeight * shipFlockingVector;

                ship.Angle = MathF.Atan2(steeringVector.Y, steeringVector.X);
            }
        }

        private Vector2 FleetCenterNaive(Ship except = null)
        {
            Vector2 accumlator = Vector2.Zero;

            foreach (var ship in Ships.Where(s => s != except))
                accumlator += ship.Position;

            accumlator /= Ships.Count;

            return accumlator;
        }

        private Vector2 Cohesion(Ship ship, int maximumDistance)
        {
            var exclusiveCenter = Vector2.Zero;
            int shipsIncluded = 0;
            foreach (var shipOther in Ships)
            {
                if (shipOther != ship)
                {
                    var distance = Vector2.Distance(ship.Position, shipOther.Position);
                    if (distance < maximumDistance)
                    {
                        exclusiveCenter += shipOther.Position;
                        shipsIncluded++;
                    }
                }
            }

            if (shipsIncluded > 0)
            {
                exclusiveCenter /= shipsIncluded;
                var relative = exclusiveCenter - ship.Position;
                var distance = Vector2.Distance(ship.Position, exclusiveCenter);

                return Vector2.Normalize(relative) * distance;
            }
            else
                return Vector2.Zero;
        }

        private Vector2 Separation(Ship ship, int minimumDistance)
        {
            var accumulator = Vector2.Zero;
            foreach (var shipOther in Ships)
            {
                if (shipOther != ship)
                {
                    var distance = Vector2.Distance(ship.Position, shipOther.Position);
                    if (distance < minimumDistance)
                    {
                        if (distance < 1)
                            distance = 1;

                        accumulator += (ship.Position - shipOther.Position) / (distance* distance);
                        //accumulator -= (shipOther.Position - ship.Position);
                    }
                }
            }

            return accumulator;
        }

        private Vector2 Alignment(Ship ship)
        {
            var accumulator = Vector2.Zero;
            foreach (var shipOther in Ships)
                accumulator += shipOther.Momentum;

            return accumulator / (Ships.Count-1);
        }
    }
}