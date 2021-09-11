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
            }
        }

        public float ThrustAmount { get; set; }
        public float Drag { get; set; }

        public bool Abandoned { get; set; }
        public Fleet AbandonedByFleet { get; set; }
        public long AbandonedTime { get; set; }

        protected bool IsOOB = false;
        public long TimeDeath = 0;

        public int ShieldStrength { get; set; }
        public long ShieldExpiration { get; internal set; }

        public Ship(World world): base(world)
        {
            Size = 70;
            Drag = World.Hook.Drag;
        }

        public override void Destroy()
        {
            base.Destroy();

            if (Fleet?.Ships?.Contains(this) ?? false)
                Fleet.Ships.Remove(this);
        }

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
        
        public void Die(Player player, Fleet fleet, ShipWeaponBullet bullet)
        {
            if (player != null)
                World.Scoring.ShipDied(player, this.Fleet?.Owner, this);

            fleet?.KilledShip(this);
            base.Die();

            if (!(this.GetType() == typeof(Fish))
                && !this.Abandoned)
                Boom.FromShip(this);

            if (this.Fleet != null)
                this.Fleet.ShipDeath(player, this, bullet);
        }

        public override void CollisionExecute(WorldBody projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
            {
                if (this.PendingDestruction)
                    return;
                if (bullet.Consumed || this.PendingDestruction)
                    return;
                
                bullet.Consumed = true;

                var fleet = bullet?.OwnedByFleet;
                var player = fleet?.Owner;

                var takesDamage = !Fleet?.Owner?.IsInvulnerable ?? true;
                if (takesDamage && ShieldStrength > 0)
                {
                    if (projectedBody is ShipWeaponSeeker)
                        ShieldStrength = 0;
                    else
                        ShieldStrength--;

                    takesDamage = false;
                }

                if (takesDamage)
                    Die(player, fleet, bullet);
            }
        }

        public override CollisionResponse CanCollide(WorldBody projectedBody)
        {
            if (PendingDestruction)
                return new CollisionResponse(false);

            // ship-to-ship collisions
            /*if (projectedBody is Ship ship)
                if (ship.Fleet != null && this.Fleet != null && this.Fleet != ship.Fleet)
                    return new CollisionResponse(true, true);*/


            if (projectedBody is ShipWeaponBullet bullet)
            {
                //if (projectedBody is ShipWeaponSeeker && this.Abandoned)
                //    return new CollisionResponse(false);

                if (bullet.Consumed)
                    return new CollisionResponse(false);

                // if it came from this fleet
                if (bullet.OwnedByFleet == this?.Fleet)
                    return new CollisionResponse(false);

                // if it came from this fleet
                if (bullet.OwnedByFleet == this?.AbandonedByFleet
                    && World.Time < (this.AbandonedTime + World.Hook.AbandonBuffer))
                    return new CollisionResponse(false);

                // team mode ensures that bullets of like colors do no harm
                if (World.Hook.TeamMode && bullet.Color == this.Color)
                    return new CollisionResponse(false);

                return new CollisionResponse(true, false);
            }

            if (!this.Abandoned)
            {
                // TODO: do we still need this beast?
                if (projectedBody is PickupBase
                    || projectedBody is HasteToken
                    || projectedBody is SystemActors.CTF.Base
                    || projectedBody is SystemActors.CTF.Flag)

                    return new CollisionResponse(true, false);;
            }

            return base.CanCollide(projectedBody);
        }

        protected override void Update(float dt)
        {

            if (Abandoned && TimeDeath == 0)
                TimeDeath = World.Time + 20000;

            if (TimeDeath > 0 && World.Time > TimeDeath)
                Die(null, null, null);

            if (ShieldStrength > 0 && World.Time > ShieldExpiration)
                ShieldStrength = 0;

            var thrust = new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * (ThrustAmount / 40f);

            LinearVelocity = (LinearVelocity + (thrust * dt)) * (1f - Drag*dt);

            base.Update(dt);
        }
    }
}
