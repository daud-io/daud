namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Pickups;
    using Game.Engine.Core.Weapons;
    using System;
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
        public Fleet AbandonedByFleet { get; set; }
        public long AbandonedTime { get; set; }

        protected bool IsOOB = false;
        private long TimeDeath = 0;

        public Ship()
        {
            Size = 10;
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

        public override void Init(World world)
        {
            base.Init(world);

            Health = MaxHealth;
            Drag = World.Hook.Drag;

            this.Group = this.Fleet;
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
            
            var entries =  World.Leaderboard.Entries;

            if (Abandoned && AbandonedByFleet.PendingDestruction) {
                Die(null, null, null);
            }

            /*
            if (Abandoned && TimeDeath == 0)
                TimeDeath = World.Time + 20000;

            if (TimeDeath > 0 && World.Time > TimeDeath)
                Die(null, null, null);*/

            Health = Math.Max(Math.Min(Health, MaxHealth), 0) + HealthRegenerationPerFrame;
            //Size = (int)(SizeMinimum + (Health / MaxHealth) * (SizeMaximum - SizeMinimum));

            DoOutOfBoundsRules();

            var thrust = new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * ThrustAmount;

            if (!Abandoned) {
                Momentum = (Momentum + thrust) * Drag;
            } else {
                Momentum = Momentum * World.Hook.DragAbandoned;
            }
                

        }

        private void DoOutOfBoundsRules()
        {
            if (this.Fleet != null) {
                var oob = World.DistanceOutOfBounds(this.Fleet.FleetCenter);

                IsOOB = oob > 0;
                
                if (IsOOB && this.Fleet.DangerSince == 0) {
                    this.Fleet.DangerSince = World.Time;
                } else if (!IsOOB) {
                    this.Fleet.DangerSince = 0;
                }
                
                if (this.Fleet.DangerSince != 0 &&
                    World.Time > this.Fleet.DangerSince + World.Hook.OutOufBoundsDecayStart &&
                    Math.Floor((decimal)(World.Time - this.Fleet.DangerSince - World.Hook.OutOufBoundsDecayStart) / World.Hook.OutOufBoundsDecayInterval) != this.Fleet.DangerDecayCounter)
                {
                    this.Fleet?.Ships[this.Fleet.Ships.Count - 1]?.Die(null, null, null);
                    this.Fleet.DangerDecayCounter++;
                }
                
                //Console.WriteLine(this.Fleet.DangerSince + ", " + (World.Time + 5000));
                
                /*if (oob > World.Hook.OutOfBoundsBorder)
                    this.Momentum *= 1 - (oob / World.Hook.OutOfBoundsDecayDistance);

                if (oob > World.Hook.OutOfBoundsDeathLine)
                {
                    //Console.WriteLine("ship dying oob");
                    Die(null, null, null);
                }*/
            } else if (this.Sprite == Sprites.fish_blue ||
                       this.Sprite == Sprites.fish_cyan ||
                       this.Sprite == Sprites.fish_green ||
                       this.Sprite == Sprites.fish_orange ||
                       this.Sprite == Sprites.fish_pink ||
                       this.Sprite == Sprites.fish_red ||
                       this.Sprite == Sprites.fish_yellow) {
                var oob = World.DistanceOutOfBounds(Position);

                IsOOB = oob > 0;

                /*if (oob > World.Hook.OutOfBoundsBorder)
                    this.Momentum *= 1 - (oob / World.Hook.OutOfBoundsDecayDistance);*/

                if (oob > World.Hook.OutOfBoundsDeathLine)
                {
                    //Console.WriteLine("ship dying oob");
                    Die(null, null, null);
                }
            }
            // catch (Exception e) {}
        }
    }
}
