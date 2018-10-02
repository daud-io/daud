namespace Game.Engine.Core
{
    using System;
    using System.Numerics;

    public class Obstacle : ActorBody, ICollide
    {
        public Obstacle(World world)
        {
            this.Init(world);

            var r = new Random();

            Position = World.RandomPosition();
            Momentum = new Vector2(
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum),
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum)
            );
            Size = r.Next(300, World.Hook.ObstacleMaxSize);
            Sprite = "obstacle";
            Color = "rgba(128,128,128,.2)";
        }

        public void CollisionExecute(ProjectedBody projectedBody)
        {
        }

        public bool IsCollision(ProjectedBody projectedBody)
        {
            return Vector2.Distance(projectedBody.Position, this.Position) 
                < (projectedBody.Size + this.Size);
        }

        public override void Think()
        {
            if (World.DistanceOutOfBounds(Position) > 0)
            {
                var speed = Momentum.Length();
                Momentum = Vector2.Normalize(Vector2.Zero - Position) * speed;
            }
        }
    }
}
