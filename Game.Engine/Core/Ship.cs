﻿namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Pickups;
    using Game.Engine.Core.Weapons;
    using System;
    using System.Numerics;

    public class Ship : Body, ICollide
    {
        public Fleet Fleet { 
            get 
            {
                return this.Group as Fleet;
            }
            set
            {
                this.Group = value;
            }
        }

        public int SizeMinimum { get; set; }
        public int SizeMaximum { get; set; }

        public float ThrustAmount { get; set; }
        public float Drag { get; set; }

        public bool Abandoned { get; set; }
        public Fleet AbandonedByFleet { get; set; }
        public long AbandonedTime { get; set; }

        protected bool IsOOB = false;
        public long TimeDeath = 0;

        public Ship(World world): base(world)
        {
            Size = 70;
            Drag = World.Hook.Drag;
        }

        public int ShieldStrength { get; set; }

        public Sprites BulletSprite
        {
            get
            {
                switch (Sprite)
                {
                    case Sprites.ship_cyan: return Sprites.bullet_cyan;
                    case Sprites.ship_blue: return Sprites.bullet_blue;
                    case Sprites.ship_green: return Sprites.bullet_green;
                    case Sprites.ship_orange: return Sprites.bullet_orange;
                    case Sprites.ship_pink: return Sprites.bullet_pink;
                    case Sprites.ship_red: return Sprites.bullet_red;
                    case Sprites.ship_yellow: return Sprites.bullet_yellow;
                    case Sprites.ship_secret: return Sprites.bullet_yellow;
                    case Sprites.ship_zed: return Sprites.bullet_red;
                    default: return Sprites.bullet;
                }
            }
        }

        public override void Destroy()
        {
            if (!(this is Fish)
                && !(this.Sprite == Sprites.ship_gray)
            )
                Boom.FromShip(this);

            base.Destroy();

            if (Fleet?.Ships?.Contains(this) ?? false)
                Fleet.Ships.Remove(this);
        }

        public void Die(Player player, Fleet fleet, ShipWeaponBullet bullet)
        {
            if (player != null)
                World.Scoring.ShipDied(player, this.Fleet?.Owner, this);

            fleet?.KilledShip(this);
            Die();

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
                        if (projectedBody is ShipWeaponSeeker)
                            this.ShieldStrength = 0;
                        else
                            this.ShieldStrength--;

                        takesDamage = false;
                    }
                }
                else
                    takesDamage = !this.Fleet?.Owner?.IsInvulnerable ?? true;

                if (takesDamage)
                    Die(player, fleet, bullet);
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

                // if it came from this fleet
                if (bullet.OwnedByFleet == this?.AbandonedByFleet
                    && World.Time < (this.AbandonedTime + World.Hook.AbandonBuffer))
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
                    || projectedBody is HasteToken
                    || projectedBody is SystemActors.CTF.Base
                    || projectedBody is SystemActors.CTF.Flag)
                    return ((Vector2.Distance(projectedBody.Position, this.Position)
                            <= this.Size + projectedBody.Size));
            }

            return false;
        }

        protected override void Update()
        {

            if (Abandoned && TimeDeath == 0)
                TimeDeath = World.Time + 20000;

            if (TimeDeath > 0 && World.Time > TimeDeath)
                Die(null, null, null);

            DoOutOfBoundsRules();

            var thrust = new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * ThrustAmount;

            LinearVelocity = (LinearVelocity + thrust) * Drag;
        }

        private void DoOutOfBoundsRules()
        {
            var oob = World.DistanceOutOfBounds(Position);

            IsOOB = oob > 0;

            if (oob > World.Hook.OutOfBoundsBorder)
                this.LinearVelocity *= 1 - (oob / World.Hook.OutOfBoundsDecayDistance);

            if (oob > World.Hook.OutOfBoundsDeathLine)
            {
                //Console.WriteLine("ship dying oob");
                Die(null, null, null);
            }
        }
    }
}
