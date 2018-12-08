namespace Game.Engine.Core
{
    using System;
    using System.Numerics;

    public class Obstacle : ActorBody, ICollide
    {
        private Vector2 IdealMomentum = Vector2.Zero;
        private float Multiplier = 1;
        public void CollisionExecute(Body projectedBody)
        {
        }

        public bool IsCollision(Body projectedBody)
        {
            var isHit = Vector2.Distance(projectedBody.Position, this.Position)
                < (projectedBody.Size + this.Size);

            return isHit;
        }

        public override void Init(World world)
        {

            World = world;
            var r = new Random();
            Position = World.RandomPosition();
            IdealMomentum = new Vector2(
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum),
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum)
            );
            Size = r.Next(World.Hook.ObstacleMinSize, World.Hook.ObstacleMaxSize);
            Sprite = Sprites.obstacle;
            Color = "rgba(128,128,128,.2)";

            base.Init(world);
        }

        public override void Think()
        {
            base.Think();


            if (World.DistanceOutOfBounds(Position) > 0)
            {
                var speed = IdealMomentum.Length();
                speed *= Multiplier;

                if (Position != Vector2.Zero)
                    IdealMomentum = Vector2.Normalize(Vector2.Zero - Position) * speed;
            }

            if (Math.Abs(World.Hook.ObstacleMaxMomentumWeatherMultiplier - Multiplier) < 0.02)
                Multiplier = World.Hook.ObstacleMaxMomentumWeatherMultiplier;
            else
                Multiplier = Multiplier * 0.97f + World.Hook.ObstacleMaxMomentumWeatherMultiplier * 0.03f;

            Momentum = IdealMomentum * Multiplier;
        }
    }
}
