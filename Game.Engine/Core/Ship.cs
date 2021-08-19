namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Pickups;
    using Game.Engine.Core.Weapons;
    using Game.Engine.Physics;
    using System;
    using System.Numerics;

    public class Ship : WorldBody
    {
        public Fleet Fleet { 
            get 
            {
                return this.Group as Fleet;
            }
            set
            {
                this.Group = value;
                OverrideBodyProperties(ref World.BodyProperties[this.BodyHandle]);
            }
        }

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

        private void OverrideBodyProperties(ref WorldBodyProperties properties)
        {
            
        }
        
        public override void Destroy()
        {
            if (!(this.GetType() == typeof(Fish))
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

        public override void CollisionExecute(WorldBody projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
            {
                if (bullet.Consumed)
                    return;

                var fleet = bullet?.OwnedByFleet;
                var player = fleet?.Owner;

                var takesDamage = true;
                if (this.Fleet?.Owner?.IsShielded ?? false)
                {
                    if (this.ShieldStrength == 0)
                        takesDamage = true;
                    else
                    {
                        if (projectedBody is ShipWeaponSeeker)
                        {
                            this.ShieldStrength = 0;
                            bullet.Consumed = true;
                        }
                        else
                            this.ShieldStrength--;

                        takesDamage = false;
                    }
                }
                else
                {
                    bullet.Consumed = true;

                    takesDamage = !this.Fleet?.Owner?.IsInvulnerable ?? true;
                }

                if (takesDamage)
                    Die(player, fleet, bullet);
            }
        }

        public override bool IsCollision(WorldBody projectedBody)
        {
            if (PendingDestruction)
                return false;

            if (projectedBody is Ship ship)
                return ship.Fleet != null && this.Fleet != null && this.Fleet != ship.Fleet;

            if (projectedBody is ShipWeaponBullet bullet)
            {
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

                return true;
            }

            if (!this.Abandoned)
            {
                if (projectedBody is PickupBase
                    || projectedBody is HasteToken
                    || projectedBody is SystemActors.CTF.Base
                    || projectedBody is SystemActors.CTF.Flag)
                    return true;
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

            /*if (oob > World.Hook.OutOfBoundsBorder)
                this.LinearVelocity *= 1 - (oob / World.Hook.OutOfBoundsDecayDistance);

            if (oob > World.Hook.OutOfBoundsDeathLine)
            {
                //Console.WriteLine("ship dying oob");
                Die(null, null, null);
            }*/
        }
    }
}
