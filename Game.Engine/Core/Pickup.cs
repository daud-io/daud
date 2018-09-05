namespace Game.Engine.Core
{
    using System;
    using System.Numerics;

    public class Pickup : ActorBody, ICollide
    {
        public string BulletSprite { get; set; }

        public Pickup(World world)
        {
            this.Init(world);

            Size = 100;
            Sprite = "seeker_pickup";
            BulletSprite = "seeker";
            Color = "rgba(128,128,128,.2)";
            Randomize();
        }

        public void Randomize()
        {
            var r = new Random();
            Position = World.RandomPosition();
            Momentum = new Vector2(
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum),
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum)
            );
        }

        public void CollisionExecute(ProjectedBody projectedBody)
        {
            var ship = projectedBody as Ship;
            var fleet = ship.Fleet;

            if (fleet != null)
            {
                // powerup the fleet
                fleet.Pickup = this;
                Randomize();
            }
        }

        public bool IsCollision(ProjectedBody projectedBody)
        {
            if (projectedBody is Ship ship)
            {
                if (ship.Abandoned)
                    return false;

                return Vector2.Distance(projectedBody.Position, this.Position)
                    < (projectedBody.Size + this.Size);
            }
            return false;
        }

        public override void Step()
        {
            AngularVelocity = 0.005f;

            if (World.DistanceOutOfBounds(Position) > 0)
            {
                var speed = Momentum.Length();
                Momentum = Vector2.Normalize(Vector2.Zero - Position) * speed;
            }
        }
    }
}
