namespace Game.Engine.Core
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class Ship : ActorBody, ICollide
    {
        public virtual int HealthHitCost { get => World.Hook.HealthHitCost; }
        public virtual int MaxHealth { get => World.Hook.MaxHealth; }
        public virtual float HealthRegenerationPerFrame { get => World.Hook.HealthRegenerationPerFrame; }

        public Fleet Fleet { get; set; }

        public float Health { get; set; }
        public int SizeMinimum { get; set; }
        public int SizeMaximum { get; set; }

        public float ThrustAmount { get; set; }
        public float Drag { get; set; }

        public bool Abandoned { get; set; }

        protected bool IsOOB = false;
        private long TimeDeath = 0;

        public override void Init(World world)
        {
            base.Init(world);

            SizeMinimum = 90;
            SizeMaximum = 90;
            Health = MaxHealth;
            Drag = World.Hook.Drag;

            this.Group = this.Fleet;
        }

        public override void Destroy()
        {
            base.Destroy();
            if (Fleet?.Ships?.Contains(this) ?? false)
                Fleet.Ships.Remove(this);
        }

        private void Die(Player player, Fleet fleet, Bullet bullet)
        {
            if (player != null)
                player.Score += 1;

            if (fleet != null)
            {
                var random = new Random();
                var threshold = fleet.Ships.Count * World.Hook.ShipGainBySizeM + World.Hook.ShipGainBySizeB;
                if (random.NextDouble() < threshold)
                    if (fleet?.Ships?.Any() ?? false)
                        fleet.AddShip();
            }

            PendingDestruction = true;

            if (this.Fleet != null)
                this.Fleet.ShipDeath(player, this, bullet);

        }

        public virtual void CollisionExecute(Body projectedBody)
        {
            var bullet = projectedBody as Bullet;
            var fleet = bullet?.OwnedByFleet;
            var player = fleet?.Owner;
            bullet.Consumed = true;

            if (!this.Fleet?.Owner?.IsInvulnerable ?? true)
            {
                Health -= HealthHitCost;

                if (Health <= 0)
                    Die(player, fleet, bullet);
            }
        }

        public bool IsCollision(Body projectedBody)
        {
            if (PendingDestruction)
                return false;

            if (projectedBody is Bullet bullet)
            {
                // avoid "piercing" shots
                if (bullet.Consumed)
                    return false;

                // if it came from this fleet
                if (bullet.OwnedByFleet == this?.Fleet)
                    return false;

                // team mode ensures that bullets of like colors do no harm
                if (World.Hook.TeamMode && bullet.Color == this.Color)
                    return false;

                // did it actually hit
                if ((Vector2.Distance(projectedBody.Position, this.Position)
                        <= this.Size + projectedBody.Size))
                    return true;
            }

            return false;
        }

        public override void Think()
        {
            base.Think();

            if (Abandoned && TimeDeath == 0)
                TimeDeath = World.Time + 20000;

            if (TimeDeath > 0 && World.Time > TimeDeath)
                Die(null, null, null);

            Health = Math.Max(Math.Min(Health, MaxHealth), 0) + HealthRegenerationPerFrame;
            Size = (int)(SizeMinimum + (Health / MaxHealth) * (SizeMaximum-SizeMinimum));

            DoOutOfBoundsRules();

            var thrust = new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * ThrustAmount;

            Momentum = (Momentum + thrust) * Drag;

        }

        private void DoOutOfBoundsRules()
        {
            var oob = World.DistanceOutOfBounds(Position);

            IsOOB = oob > 0;

            if (oob > World.Hook.OutOfBoundsBorder)
                this.Momentum *= 1 - (oob / World.Hook.OutOfBoundsDecayDistance);

            if (oob > 700)
                Die(null, null, null);
        }
    }   
}
