namespace Game.Engine.Core
{
    using Game.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public class Fleet : ActorBody
    {
        [JsonIgnore]
        public virtual int ShootCooldownTime { get => World.Hook.ShootCooldownTime; }
        [JsonIgnore]
        public virtual float BaseThrust { get => World.Hook.BaseThrust; }
        [JsonIgnore]
        public virtual float MaxSpeed { get => World.Hook.MaxSpeed; }
        [JsonIgnore]
        public virtual float MaxSpeedBoost { get => World.Hook.MaxSpeedBoost; }

        [JsonIgnore]
        public Player Owner { get; set; }

        [JsonIgnore]
        public bool BoostRequested { get; set; }
        [JsonIgnore]
        public bool ShootRequested { get; set; }

        [JsonIgnore]
        public long TimeReloaded { get; set; } = 0;

        [JsonIgnore]
        public List<Ship> Ships { get; set; } = new List<Ship>();

        private void Die(Player player, Fleet fleet, Bullet bullet)
        {
            player.Score += 55;

            player.SendMessage($"You Killed {this.Owner.Name}");
            this.Owner.SendMessage($"Killed by {player.Name}");

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
                Die(player, bullet.Owner.Owner, bullet);

        }

        public Ship AddShip(Vector2 offset)
        {
            var ship = new Ship()
            {
                Owner = this,
                Position = Vector2.Add(this.Position, offset),
                Momentum = this.Momentum
            };
            
            ship.Init(World);
            Ships.Add(ship);

            return ship;
        }

        public override void Init(World world)
        {
            this.Position = world.RandomPosition();

            base.Init(world);

            this.AddShip(new Vector2(100, 100));
            this.AddShip(new Vector2(-100, -100));

        }

        public override void Step()
        {
            /*Health = Math.Max(Math.Min(Health, MaxHealth), 0);

            Size = (int)(SizeMinimum + (Health / MaxHealth) * (SizeMaximum-SizeMinimum));
            */

            var isShooting = ShootRequested && World.Time >= TimeReloaded;
            var isBoosting = BoostRequested;

            float thrustAmount = BaseThrust;

            if (isBoosting)
                thrustAmount *= 2;

            var Thrust =
                Vector2.Transform(
                    new Vector2(thrustAmount, 0),
                    Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), Angle)
                );

            float speedLimit = isBoosting
                ? MaxSpeedBoost
                : MaxSpeed;

            var x = Vector2.Add(this.Momentum, Thrust);
            var currentSpeed = Math.Abs(Vector2.Distance(x, Vector2.Zero));
            if (currentSpeed > speedLimit)
                x = Vector2.Multiply(Vector2.Normalize(x), ((speedLimit + 3 * currentSpeed) / 4));

            Momentum = x;

            foreach (var ship in Ships)
            {
                ship.Momentum = x;
                ship.Angle = Angle;
            }

            if (isShooting)
            {
                TimeReloaded = World.Time + ShootCooldownTime;

                foreach (var ship in Ships)
                    Bullet.FireFrom(ship);
            }

            /*Health = Math.Min(Health + HealthRegenerationPerFrame, MaxHealth);
            this.Size = (int)(60 + (Health / MaxHealth * 90));*/
        }
    }   
}
