namespace Game.Engine.Core
{
    using Game.Models;
    using Newtonsoft.Json;
    using System;
    using System.Numerics;

    public class Fleet : ActorBody, ICollide
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
        public virtual int MaxHealth { get => World.Hook.MaxHealth; }
        [JsonIgnore]
        public virtual float HealthRegenerationPerFrame { get => World.Hook.HealthRegenerationPerFrame; }
        [JsonIgnore]
        public virtual int HealthHitCost { get => World.Hook.HealthHitCost; }

        [JsonIgnore]
        public Player Owner { get; set; }

        [JsonIgnore]
        public float Health { get; set; }

        [JsonIgnore]
        public int SizeMinimum { get; set; }
        [JsonIgnore]
        public int SizeMaximum { get; set; }

        [JsonIgnore]
        public bool BoostRequested { get; set; }
        [JsonIgnore]
        public bool ShootRequested { get; set; }

        [JsonIgnore]
        public long TimeReloaded { get; set; } = 0;

        private void Die(Player player, Fleet fleet, Bullet bullet)
        {
            player.Score += 55;

            player.SendMessage($"You Killed {this.Owner.Name}");
            this.Owner.SendMessage($"Killed by {player.Name}");

            this.Owner.Die();
            Deinit();
        }


        public void CollisionExecute(ProjectedBody projectedBody)
        {
            var bullet = projectedBody as Bullet;
            var fleet = bullet?.Owner;
            var player = fleet?.Owner;

            Health -= HealthHitCost;

            if (Health <= 0)
                Die(player, fleet, bullet);
        }

        public bool IsCollision(ProjectedBody projectedBody)
        {

            if (projectedBody is Bullet bullet)
            {
                if (bullet.Owner == this)
                    return false;

                if (World.Hook.TeamMode && bullet.Color == this.Color)
                    return false;

                if ((Vector2.Distance(projectedBody.Position, this.Position)
                        <= this.Size + projectedBody.Size))
                    return true;
            }

            return false;
        }

        public override void Init(World world)
        {
            base.Init(world);
            this.Health = MaxHealth;

            var random = new Random();
            this.Position = new Vector2(
                random.Next(-world.WorldSize, world.WorldSize),
                random.Next(-world.WorldSize, world.WorldSize)
            );
        }

        public override void Step()
        {
            Health = Math.Max(Math.Min(Health, MaxHealth), 0);

            Size = (int)(SizeMinimum + (Health / MaxHealth) * (SizeMaximum-SizeMinimum));

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

            if (!Momentum.Equals(x))
            {
                Momentum = x;
                DefinitionTime = World.Time;
            }

            if (isShooting)
            {
                TimeReloaded = World.Time + ShootCooldownTime;

                Bullet.FireFrom(this);
            }

            Health = Math.Min(Health + HealthRegenerationPerFrame, MaxHealth);
            this.Size = (int)(60 + (Health / MaxHealth * 90));
        }
    }   
}
