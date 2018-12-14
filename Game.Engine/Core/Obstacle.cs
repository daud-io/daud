namespace Game.Engine.Core
{
    using Game.API.Common;
    using System;
    using System.Numerics;

    public class Obstacle : ActorBody, ICollide, ILifeCycle
    {
        private Vector2 IdealMomentum = Vector2.Zero;
        private float Multiplier = 1;
        private float GrowthRate = 0;
        private long DieByTime = 0;
        private float IdealSize = 0;

        private int TargetSize = 0;


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
            IdealSize = 0;
            Size = 0;
            Sprite = Sprites.obstacle;
            Color = "rgba(128,128,128,.2)";

            base.Init(world);
        }

        public override void Think()
        {
            base.Think();

            if (World.DistanceOutOfBounds(Position, World.Hook.ObstacleBorderBuffer) > 0)
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

            if (GrowthRate != 0)
                IdealSize += (GrowthRate * (float)World.LastStepSize);

            if (DieByTime > 0 && DieByTime < World.Time)
                this.PendingDestruction = true;

            if (GrowthRate > 0 && IdealSize > TargetSize)
                GrowthRate = 0;

            //Console.WriteLine(Size);
            Size = (int)IdealSize;
        }

        public void Die()
        {
            long LengthOfDeath = World.Hook.LifecycleDuration;
            DieByTime = World.Time + LengthOfDeath;
            GrowthRate = -1 * (float)Size / (float)LengthOfDeath; // shrink
        }

        public void Spawn()
        {
            var r = new Random();
            this.TargetSize = r.Next(World.Hook.ObstacleMinSize, World.Hook.ObstacleMaxSize);
            GrowthRate = (float)this.TargetSize / (float)World.Hook.LifecycleDuration; // grow
        }
    }
}
