namespace Game.Engine.Core
{
    using Game.Engine.Core.Pickups;
    using Game.Engine.Core.SystemActors.CTF;
    using Game.Engine.Core.Weapons;
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

        public int ShieldStrength { get; set; }

        public override void Init(World world)
        {
            base.Init(world);

            Size = 70;
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

        private void Die(Player player, Fleet fleet, ShipWeaponBullet bullet)
        {
            if (player != null)
                World.Scoring.ShipDied(player, this.Fleet?.Owner, this);

            fleet?.KilledShip();

            PendingDestruction = true;

            if (this.Fleet != null)
                this.Fleet.ShipDeath(player, this, bullet);
        }

        public virtual void CollisionExecute(Body projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
            {
                var fleet = bullet?.OwnedByFleet;
                var player = fleet?.Owner;
                bullet.Consumed = true;

                var takesDamage = true;
                if (this.Fleet?.Owner?.IsShielded ?? false)
                {
                    if (this.ShieldStrength == 0)
                        takesDamage = true;
                    else
                    {
                        this.ShieldStrength--;
                        takesDamage = false;
                    }
                }
                else
                    takesDamage = !this.Fleet?.Owner?.IsInvulnerable ?? true;

                if (takesDamage)
                {
                    Health -= HealthHitCost;

                    if (Health <= 0)
                        Die(player, fleet, bullet);
                }
            }
        }

        public bool IsCollision(Body projectedBody)
        {
            if (PendingDestruction)
                return false;

            if (projectedBody is ShipWeaponBullet bullet)
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

            if (!this.Abandoned)
            {
                if (projectedBody is PickupBase
                    || projectedBody is SystemActors.CTF.Base
                    || projectedBody is SystemActors.CTF.Flag)
                    return ((Vector2.Distance(projectedBody.Position, this.Position)
                            <= this.Size + projectedBody.Size));
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
            //Size = (int)(SizeMinimum + (Health / MaxHealth) * (SizeMaximum - SizeMinimum));

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

            if (oob > World.Hook.OutOfBoundsDeathLine)
            {
                //Console.WriteLine("ship dying oob");
                Die(null, null, null);
            }
        }
    }
}
