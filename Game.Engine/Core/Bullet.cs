namespace Game.Engine.Core
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class Bullet : ActorBody, ICollide
    {
        public Fleet OwnedByFleet { get; set; }
        public long TimeDeath { get; set; }
        public long TimeBirth { get; set; }

        public bool Seeker { get; set; } = false;

        public float ThrustAmount { get; set; }
        public float ThrustAngle { get; set; }

        public float Drag { get => World.Hook.Drag; }

        public bool Consumed { get; set; }

        public static Bullet FireFrom(Ship ship)
        {
            var world = ship.World;
            var bulletOrigin = ship.Position
                + new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle)) * ship.Size;

            bool isSeeker = ship.Fleet.Pickup != null;
            float lifeMultiplier = isSeeker
                ? world.Hook.SeekerLifeMultiplier
                : 1;

            var momentum =
                new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle)) * Vector2.Distance(ship.Momentum, Vector2.Zero);

            if (isSeeker)
                momentum /= 2.0f;

            var bullet = new Bullet
            {
                TimeDeath = world.Time + (long)(world.Hook.BulletLife * lifeMultiplier),
                Momentum = momentum,
                Position = bulletOrigin,
                Angle = ship.Angle,
                OwnedByFleet = ship.Fleet,
                Sprite = ship.Fleet.Pickup?.BulletSprite ?? ship.Fleet.BulletSprite,
                Size = ship.Fleet.Pickup?.Size ?? 20,
                Color = ship.Color,
                Seeker = isSeeker,
                ThrustAmount = ship.Fleet.Ships.Count() * ship.Fleet.ShotThrustM + ship.Fleet.ShotThrustB,
                TimeBirth = world.Time
            };
            
            return bullet;
        }

        public override void Think()
        {
            base.Think();

            if (Seeker)
            {
                Ship target = null;
                if (World.Time > TimeBirth + World.Hook.SeekerDelay)
                {
                    var targets = World.BodiesNear(this.Position, World.Hook.SeekerRange, offsetSize: true);

                    target = targets
                        .OfType<Ship>()
                        .Where(s => s.Fleet != OwnedByFleet)
                        .Where(s => s.Fleet != null)
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
            else
            {
                ThrustAngle = Angle;
                var thrust = new Vector2(MathF.Cos(ThrustAngle), MathF.Sin(ThrustAngle)) * ThrustAmount * 10;
                Momentum = thrust;
            }

            if (World.Time >= TimeDeath)
                PendingDestruction = true;
        }

        protected override void Collided(ICollide otherObject)
        {
            TimeDeath = World.Time;
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

            if (!this.Seeker)
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
