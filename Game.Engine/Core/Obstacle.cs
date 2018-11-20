namespace Game.Engine.Core
{
    using System;
    using System.Numerics;

    public class Obstacle : ActorBody, ICollide
    {
        public void CollisionExecute(Body projectedBody)
        {
        }

        public bool IsCollision(Body projectedBody)
        {
            return Vector2.Distance(projectedBody.Position, this.Position)
                < (projectedBody.Size + this.Size);
        }

        public override void Init(World world)
        {

            World = world;
            var r = new Random();
            Position = World.RandomPosition();
            Momentum = new Vector2(
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum),
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum)
            );
            Size = r.Next(300, World.Hook.ObstacleMaxSize);
            Sprite = Sprites.obstacle;
            Color = "rgba(128,128,128,.2)";
            base.Init(world);
        }

        public override void Think()
        {
            base.Think();

            if (World.DistanceOutOfBounds(Position) > 0)
            {
                var speed = Momentum.Length();
                if (Position != Vector2.Zero)
                    Momentum = Vector2.Normalize(Vector2.Zero - Position) * speed;
            }
        }
    }
}
