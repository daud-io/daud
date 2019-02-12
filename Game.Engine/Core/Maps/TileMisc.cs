using Game.Engine.Core.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Game.Engine.Core.Maps
{
    public class TileMisc : ActorBody, ICollide
    {
        public bool IsDeadly { get; set; } = false;
        public bool IsObstacle { get; set; } = false;
        public bool IsBouncy { get; set; } = false;
        public float Drag { get; set; } = 0;
        public bool IsTurret { get; set; } = false;
        private long ShotCooldown = 0;

        private List<ShipWeaponBullet> NewBullets = new List<ShipWeaponBullet>();

        public TileMisc()
        {
            MaximumCleanTime = 100000;
            IsStatic = true;
        }

        public bool IsCollision(Body projectedBody)
        {
            if (!IsDeadly && !IsObstacle && Drag == 0)
                return false;
            else
            {
                if ((IsObstacle && projectedBody is ShipWeaponBullet)
                    || ((IsDeadly || Drag != 0) && projectedBody is Ship))
                {
                    return Vector2.Distance(projectedBody.Position, Position) < projectedBody.Size + Size;
                }
                else
                    return false;
            }
        }


        public void CollisionExecute(Body projectedBody)
        {
            if (projectedBody is Ship ship)
            {
                if (IsDeadly
                    && ship.Fleet != null
                    && !ship.Fleet.Owner.IsInvulnerable
                    && ship.Fleet.BoostUntil <= World.Time - 1000
                    )
                    ship.Fleet.AbandonShip(ship);

                if (IsBouncy)
                    Collide(this, projectedBody);

                if (Drag != 0)
                    ship.Momentum *= 1 - Drag;
            }
        }


        public override void Think()
        {
            //base.Think();

            if (IsTurret && ShotCooldown < World.Time)
            {
                var target = World.BodiesNear(this.Position, 2500)
                    .OfType<Ship>()
                    .Where(s => s.Abandoned == false)
                    .Where(s => s.Sprite != API.Common.Sprites.fish)
                    .FirstOrDefault();

                if (target != null)
                {
                    var bullet = new ShipWeaponBullet();
                    var toTarget = target.Position - Position;
                    bullet.FireFrom(this, MathF.Atan2(toTarget.Y, toTarget.X));

                    NewBullets.Add(bullet);

                    ShotCooldown = World.Time + 300;
                }
            }
        }

        public override void CreateDestroy()
        {
            base.CreateDestroy();

            foreach (var bullet in NewBullets)
                bullet.Init(World);

            NewBullets.Clear();
        }

        public void Collide(Body bthis, Body ball)
        {
            // get the mtd
            Vector2 delta = bthis.Position - ball.Position;
            float r = bthis.Size + ball.Size;
            float dist2 = Vector2.Dot(delta, delta);

            if (dist2 > r * r) return; // they aren't colliding

            float d = delta.Length();

            Vector2 mtd;
            if (d != 0.0f)
                mtd = delta * (((bthis.Size + ball.Size) - d) / d); // minimum translation distance to push balls apart after intersecting
            else // Special case. Balls are exactly on top of eachother.  Don't want to divide by zero.
            {
                d = ball.Size + bthis.Size - 1.0f;
                delta = new Vector2(ball.Size + bthis.Size, 0.0f);

                mtd = delta * (((bthis.Size + ball.Size) - d) / d);
            }

            float ballMass = 4f / 3f * MathF.PI * MathF.Pow(ball.Size + 1, 3);
            float thisMass = 4f / 3f * MathF.PI * MathF.Pow(bthis.Size + 1, 3);

            // resolve intersection
            float im1 = 1f / thisMass; // inverse mass quantities
            float im2 = 1f / ballMass;

            // push-pull them apart
            //bthis.Position = bthis.Position + (mtd * (im1 / (im1 + im2)));
            ball.Position = ball.Position - (mtd * (im2 / (im1 + im2)));

            // impact speed
            Vector2 v = bthis.Momentum - ball.Momentum;
            float vn = Vector2.Dot(v, Vector2.Normalize(mtd));

            // sphere intersecting but moving away from each other already
            if (vn > 0.0f) return;

            // collision impulse
            const float restitution = -0.98f;
            float i = (-(1.0f + restitution) * vn) / (im1 + im2);
            Vector2 impulse = mtd * i;

            // change in momentum
            //bthis.Momentum = bthis.Momentum + (impulse * im1);
            ball.Momentum = ball.Momentum - (impulse * im2);
        }

    }
}
