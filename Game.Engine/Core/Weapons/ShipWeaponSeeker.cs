using System;
using System.Linq;
using System.Numerics;

namespace Game.Engine.Core.Weapons
{
    public class ShipWeaponSeeker : Bullet, ICollide
    {
        public override void FireFrom(Ship ship, ActorGroup group)
        {
            base.FireFrom(ship, group);

            this.Momentum /= 2.0f;
            this.TimeDeath = World.Time + (long)(World.Hook.BulletLife * World.Hook.SeekerLifeMultiplier);
            this.Sprite = API.Common.Sprites.seeker;
        }

        public override void Think()
        {
            base.Think();

            Ship target = null;
            if (World.Time > TimeBirth + World.Hook.SeekerDelay)
            {
                var targets = World.BodiesNear(this.Position, World.Hook.SeekerRange, offsetSize: true);

                target = targets
                    .OfType<Ship>()
                    .Where(s => s.Fleet != OwnedByFleet)
                    .Where(s => s.Fleet != null)
                    .Where(s => !World.Hook.TeamMode || s.Fleet?.Owner.Color != this.Color)
                    .OrderBy(s => Vector2.Distance(s.Position, Position))
                    .FirstOrDefault();
            }

            if (target != null)
            {
                var delta = target.Position - Position;
                ThrustAngle = MathF.Atan2(delta.Y, delta.X);

                Angle = MathF.Atan2(Momentum.Y, Momentum.X);
            }
            else
                ThrustAngle = Angle;

            var thrust = new Vector2(MathF.Cos(ThrustAngle), MathF.Sin(ThrustAngle)) * ThrustAmount * World.Hook.SeekerThrustMultiplier;
            Momentum = (Momentum + thrust) * Drag;
        }

        public virtual void CollisionExecute(Body projectedBody)
        {
            var bullet = projectedBody as Bullet;
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

            if (projectedBody is Bullet bullet)
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
