using System;
using System.Numerics;

namespace Game.Engine.Core.Maps
{
    public class Tile : ActorBody, ICollide
    {
        public bool IsDeadly { get; set; } = false;
        public bool IsObstacle { get; set; } = false;
        public float Drag { get; set; } = 0;


        public Tile()
        {
            MaximumCleanTime = 100000;
            IsStatic = true;
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
                else if (IsObstacle)
                    Collide(this, projectedBody);

                if (Drag != 0)
                    ship.Momentum *= 1 - Drag;
            }
        }


        public override void Think()
        {

            //base.Think();
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


        public bool IsCollision(Body projectedBody)
        {
            if (!IsDeadly && !IsObstacle && Drag == 0)
                return false;
            else
                return (projectedBody is Ship)
                    && Vector2.Distance(projectedBody.Position, Position) < projectedBody.Size + Size;
        }

    }
}
