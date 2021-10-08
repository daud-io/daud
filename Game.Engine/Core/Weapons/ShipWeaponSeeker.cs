﻿using System;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Game.Engine.Core.Weapons
{
    public class ShipWeaponSeeker : ShipWeaponBullet
    {
        public Ship DeclaredTarget { get; private set; } = null;

        public ShipWeaponSeeker(World world, Ship ship): base(world, ship)
        {
            Interlocked.Increment(ref World.ProjectileCount);
            
        }

        public override void FireFrom(Ship ship, ActorGroup group)
        {
            base.FireFrom(ship, group);

            this.LinearVelocity /= 2.0f;
            this.TimeDeath = World.Time + (long)(World.Hook.BulletLife * World.Hook.SeekerLifeMultiplier);
            this.Sprite = API.Common.Sprites.seeker;
            this.Size = 100;
        }

        protected override void Update(float dt)
        {
            var originalVelocity = LinearVelocity;

            Ship target = null;
            if (World.Time > TimeBirth + World.Hook.SeekerCycle)
            {
                var targets = World.BodiesNear(this.Position, World.Hook.SeekerRange);

                target = targets
                    .OfType<Ship>()
                    .Where(s => s.Fleet != OwnedByFleet)
                    .Where(s => s.Fleet != null)
                    .Where(s => !World.Hook.TeamMode || s.Fleet?.Owner.Color != this.Color)
                    .OrderBy(s => (!World.Hook.SeekerNegotiation
                        || (!(this.Group as ShipWeaponVolley<ShipWeaponSeeker>)
                            ?.AllWeapons
                            .OfType<ShipWeaponSeeker>()
                            .Any(w => w != this && w.DeclaredTarget == s) ?? false)
                            )
                                ? 0
                                : 1
                        )
                    .ThenBy(s => Vector2.Distance(s.Position, Position))
                    .Where(s => Vector2.Dot(originalVelocity, s.Position - Position) > 0)
                    .FirstOrDefault();

                DeclaredTarget = target;
            }

            float thrustAngle = 0;
            if (target != null)
            {
                var delta = (target.Position + (target.LinearVelocity * World.Hook.SeekerLead)) - Position;
                thrustAngle = MathF.Atan2(delta.Y, delta.X);

                Angle = thrustAngle;

            }
            else
                thrustAngle = Angle;

            var thrust = new Vector2(MathF.Cos(thrustAngle), MathF.Sin(thrustAngle)) * ThrustAmount/40 * World.Hook.SeekerThrustMultiplier;
            LinearVelocity = (originalVelocity + thrust * dt) * (1-Drag*dt);

            base.Update(dt);
        }

        public override void CollisionExecute(WorldBody projectedBody)
        {
            var bullet = projectedBody as ShipWeaponBullet;
            var fleet = bullet?.OwnedByFleet;
            var player = fleet?.Owner;

            if (bullet != null && !bullet.Consumed && !this.Consumed)
            {
                bullet.Consumed = true;
                this.Consumed = true;
                this.Die();
            }
        }

        public override CollisionResponse CanCollide(WorldBody projectedBody)
        {
            if (Consumed)
                return new CollisionResponse(false);

            if (projectedBody is ShipWeaponBullet bullet)
            {
                // avoid "piercing" shots
                if (bullet.Consumed)
                    return new CollisionResponse(false);

                // if it came from this fleet
                if (bullet.OwnedByFleet == this?.OwnedByFleet)
                    return new CollisionResponse(false);

                // team mode ensures that bullets of like colors do no harm
                if (World.Hook.TeamMode && bullet.Color == this.Color)
                    return new CollisionResponse(false);

                return new CollisionResponse(true, false);
            }

            return new CollisionResponse(false);
        }

        public override void Destroy()
        {
            Interlocked.Decrement(ref World.ProjectileCount);
            base.Destroy();
        }

    }
}
