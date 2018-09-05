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

        public override void Init(World world)
        {
            base.Init(world);

            SizeMinimum = 90;
            SizeMaximum = 90;
            Health = MaxHealth;
        }

        private void Die(Player player, Fleet fleet, Bullet bullet)
        {
            if (player != null)
                player.Score += 1;

            if (this.Fleet != null)
                this.Fleet.ShipDeath(player, this, bullet);

            if (fleet != null)
            {
                var random = new Random();
                var threshold = fleet.Ships.Count * World.Hook.ShipGainBySizeM + World.Hook.ShipGainBySizeB;
                if (random.NextDouble() < threshold)
                    if (fleet?.Ships?.Any() ?? false)
                        fleet.AddShip();
            }

            Deinit();
        }

        public void CollisionExecute(ProjectedBody projectedBody)
        {
            var bullet = projectedBody as Bullet;
            var fleet = bullet?.OwnedByFleet;
            var player = fleet?.Owner;

            Health -= HealthHitCost;

            if (Health <= 0)
                Die(player, fleet, bullet);
        }

        public bool IsCollision(ProjectedBody projectedBody)
        {
            if (projectedBody is Bullet bullet)
            {
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

        public override void Step()
        {
            base.Step();

            if (Abandoned)
            {

            };

            Health = Math.Max(Math.Min(Health, MaxHealth), 0) + HealthRegenerationPerFrame;
            Size = (int)(SizeMinimum + (Health / MaxHealth) * (SizeMaximum-SizeMinimum));

            DoOutOfBoundsRules();

            var thrust = new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * ThrustAmount;

            Momentum = (Momentum + thrust) * Drag;

        }

        private void DoOutOfBoundsRules()
        {
            var oob = World.DistanceOutOfBounds(Position);

            if (oob > World.Hook.OutOfBoundsBorder)
                this.Momentum *= 1 - (oob / World.Hook.OutOfBoundsDecayDistance);

            if (oob > 700)
                Die(null, null, null);

        }

    }   
}
