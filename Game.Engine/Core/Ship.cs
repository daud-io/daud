namespace Game.Engine.Core
{
    using Game.Models;
    using Newtonsoft.Json;
    using System;
    using System.Numerics;

    public class Ship : ActorBody, ICollide
    {
        [JsonIgnore]
        public virtual int HealthHitCost { get => World.Hook.HealthHitCost; }

        [JsonIgnore]
        public virtual int MaxHealth { get => World.Hook.MaxHealth; }

        [JsonIgnore]
        public virtual float HealthRegenerationPerFrame { get => World.Hook.HealthRegenerationPerFrame; }

        [JsonIgnore]
        public Fleet Owner { get; set; }

        [JsonIgnore]
        public float Health { get; set; }

        [JsonIgnore]
        public int SizeMinimum { get; set; }
        [JsonIgnore]
        public int SizeMaximum { get; set; }

        public Ship()
        {
        }

        public override void Init(World world)
        {
            base.Init(world);

            SizeMinimum = 90;
            SizeMaximum = 150;
            Health = MaxHealth;

        }

        private void Die(Player player, Fleet fleet, Bullet bullet)
        {
            player.Score += 1;

            this.Owner.ShipDeath(player, this, bullet);

            Deinit();
        }

        public void CollisionExecute(ProjectedBody projectedBody)
        {
            var bullet = projectedBody as Bullet;
            var ship = bullet?.Owner;
            var fleet = ship?.Owner;
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

        public override void Step()
        {
            Health = Math.Max(Math.Min(Health, MaxHealth), 0);

            Size = (int)(SizeMinimum + (Health / MaxHealth) * (SizeMaximum-SizeMinimum));

            Health = Math.Min(Health + HealthRegenerationPerFrame, MaxHealth);
            this.Size = (int)(60 + (Health / MaxHealth * 90));
        }
    }   
}
