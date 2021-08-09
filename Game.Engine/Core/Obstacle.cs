﻿namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;
    using System;
    using System.Numerics;

    public class Obstacle : ActorBody, ICollide, ILifeCycle
    {
        private Vector2 TargetMomentum = Vector2.Zero;
        private float Multiplier = 1;
        private long DieByTime = 0;
        private float IdealSize = 1;
        protected int TargetSize = 0;
        private bool SpawnPickup = false;

        public virtual void CollisionExecute(Body projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
            {
                if (!bullet.Consumed)
                {
                    TargetSize = (int)(TargetSize * 0.99f);
                    if (TargetSize < World.Hook.ObstacleMinSize)
                        this.Die();
                }
                bullet.Consumed = true;

            }
        }

        public virtual bool IsCollision(Body projectedBody)
        {
            var isHit = false;

            if (projectedBody is ShipWeaponBullet bullet)
                isHit = Vector2.Distance(projectedBody.Position, this.Position)
                    < (projectedBody.Size + this.Size);

            return isHit;
        }

        public override void Init(World world)
        {
            World = world;
            var r = new Random();
            Position = World.RandomPosition();
            TargetMomentum = new Vector2(
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum),
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum)
            );

            Sprite = Sprites.obstacle;
            Color = "rgba(128,128,128,.2)";

            this.Group = new Group()
            {
                GroupType = GroupTypes.Obstacle,
                ZIndex = 400
            };

            base.Init(world);
        }

        public override void CreateDestroy()
        {
            if (SpawnPickup)
            {
                var pickup = new Pickups.PickupShieldCannon();
                pickup.Init(World);
                pickup.Position = this.Position;
                SpawnPickup = false;
            }

            base.CreateDestroy();
        }

        public override void Think()
        {
            base.Think();

            if (World.DistanceOutOfBounds(Position, World.Hook.ObstacleBorderBuffer) > 0)
            {
                var speed = TargetMomentum.Length();
                speed *= Multiplier;

                if (Position != Vector2.Zero)
                    TargetMomentum = Vector2.Normalize(Vector2.Zero - Position) * speed;
            }

            var weatherMultiplerDelta = Math.Abs(World.Hook.ObstacleMaxMomentumWeatherMultiplier - Multiplier);
            if (weatherMultiplerDelta / World.Hook.ObstacleMaxMomentumWeatherMultiplier > 0.02)
                Multiplier = Multiplier * 0.97f + World.Hook.ObstacleMaxMomentumWeatherMultiplier * 0.03f;
            LinearVelocity = TargetMomentum * Multiplier;

            if (IdealSize > 0 && MathF.Abs(IdealSize - TargetSize) / IdealSize > 0.02f)
            {
                var step = (IdealSize * 0.97f + TargetSize * 0.03f) - IdealSize;

                if (step > 0)
                    step = MathF.Max(step, 0.03f * MathF.Abs(IdealSize - TargetSize));
                else if (step < 0)
                    step = MathF.Min(step, -0.03f * MathF.Abs(IdealSize - TargetSize));

                IdealSize += step;
            }

            if (IdealSize < World.Hook.ObstacleMinSize * 0.02)
            {
                if (World.Hook.ObstaclesSpawnShieldCannons)
                    SpawnPickup = true;
                this.PendingDestruction = true;
            }

            /* if (GrowthRate != 0)
                IdealSize += (GrowthRate * (float)World.LastStepSize);

            if (DieByTime > 0 && DieByTime < World.Time)
                this.PendingDestruction = true;

            if (GrowthRate > 0 && IdealSize > TargetSize)
                GrowthRate = 0;*/

            //Console.WriteLine(Size);
            Size = (int)IdealSize;
        }

        public void Die()
        {
            var random = new Random();
            long LengthOfDeath = World.Hook.LifecycleDuration;
            DieByTime = World.Time + LengthOfDeath;
            AngularVelocity = ((float)random.NextDouble() - 0.5f) * 0.05f;
            TargetSize = 0;

            //GrowthRate = -1 * (float)Size / (float)LengthOfDeath; // shrink
        }

        public void Spawn()
        {
            var r = new Random();
            this.TargetSize = r.Next(World.Hook.ObstacleMinSize, World.Hook.ObstacleMaxSize);
            //GrowthRate = (float)this.TargetSize / (float)World.Hook.LifecycleDuration; // grow
        }
    }
}
