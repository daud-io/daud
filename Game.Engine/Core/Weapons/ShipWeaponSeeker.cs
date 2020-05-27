﻿using System;
using System.Linq;
using System.Numerics;

namespace Game.Engine.Core.Weapons
{
    public class ShipWeaponSeeker : ShipWeaponBullet, ICollide
    {

        public Ship DeclaredTarget { get; private set; } = null;
        public float ExtraTime = 0;

        public override void FireFrom(Ship ship, ActorGroup group)
        {
            base.FireFrom(ship, group);

            this.ExtraTime = ship.Fleet.AimTarget.Length() * World.Hook.SeekerCycleM;
            this.Momentum /= 2.0f;
            this.TimeDeath = World.Time + (long)(World.Hook.BulletLife * World.Hook.SeekerLifeMultiplier);
            this.Sprite = API.Common.Sprites.seeker;
            this.Size = 100;

        }

        public override void Think()
        {
            var originalMomentum = Momentum;

            base.Think();

            Ship target = null;
            if (World.Time > TimeBirth + World.Hook.SeekerCycleB + this.ExtraTime)
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
                    .FirstOrDefault();

                DeclaredTarget = target;
            }

            float thrustAngle = 0;
            if (target != null)
            {
                var delta = (target.Position + (target.Momentum * World.Hook.SeekerLead)) - Position;
                thrustAngle = MathF.Atan2(delta.Y, delta.X);

                Angle = thrustAngle;

            }
            else
                thrustAngle = Angle;

            var thrust = new Vector2(MathF.Cos(thrustAngle), MathF.Sin(thrustAngle)) * ThrustAmount * World.Hook.SeekerThrustMultiplier;
            Momentum = (originalMomentum + thrust) * Drag;
        }

        public virtual void CollisionExecute(Body projectedBody)
        {
            var bullet = projectedBody as ShipWeaponBullet;
            var fleet = bullet?.OwnedByFleet;
            var player = fleet?.Owner;
            bullet.Consumed = true;

            this.Consumed = true;
            this.PendingDestruction = true;
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
                if (bullet.OwnedByFleet == this?.OwnedByFleet)
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
    }
}
