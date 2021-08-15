namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;
    using System;
    using System.Numerics;

    public class Obstacle : WorldBody
    {
        private Vector2 TargetVelocity = Vector2.Zero;
        private long DieByTime = 0;
        private float IdealSize = 1;
        protected int TargetSize = 0;
        public Obstacle(World world): base(world)
        {
            var r = new Random();
            Position = World.RandomPosition();
            LinearVelocity = new Vector2(
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum),
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum)
            );

            Sprite = Sprites.obstacle;
            Color = "rgba(128,128,128,.2)";

            this.Group = new Group(world)
            {
                GroupType = GroupTypes.Obstacle,
                ZIndex = 400
            };

            this.TargetSize = r.Next(World.Hook.ObstacleMinSize, World.Hook.ObstacleMaxSize);
        }

        public override void CollisionExecute(WorldBody projectedBody)
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

        public override bool IsCollision(WorldBody projectedBody)
        {
            var isHit = false;

            if (projectedBody is ShipWeaponBullet bullet)
                isHit = Vector2.Distance(projectedBody.Position, this.Position)
                    < (projectedBody.Size + this.Size);

            return isHit;
        }

        protected override void Update()
        {
            if (World.DistanceOutOfBounds(Position, World.Hook.ObstacleBorderBuffer) > 0)
            {
                var speed = LinearVelocity.Length();

                if (Position != Vector2.Zero)
                    LinearVelocity = Vector2.Normalize(Vector2.Zero - Position) * speed;
            }

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
                base.Die();
            }

            Size = (int)IdealSize;
        }

        protected override void Die()
        {
            var random = new Random();
            long LengthOfDeath = World.Hook.LifecycleDuration;
            DieByTime = World.Time + LengthOfDeath;
            AngularVelocity = ((float)random.NextDouble() - 0.5f) * 0.05f;
            TargetSize = 0;

            //GrowthRate = -1 * (float)Size / (float)LengthOfDeath; // shrink
        }
    }
}
