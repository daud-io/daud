namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;
    using System;
    using System.Numerics;

    public class Obstacle : WorldBody
    {
        private Vector2 TargetVelocity = Vector2.Zero;
        protected float Drag = 0.97f;

        public Obstacle(World world) : base(world)
        {
            var r = new Random();
            Position = World.RandomPosition();
            LinearVelocity = new Vector2(
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum),
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum)
            );

            Sprite = Sprites.sportsball;
            Color = "rgba(128,128,128,.2)";

            this.Group = new Group(world)
            {
                GroupType = GroupTypes.Obstacle,
                ZIndex = 400
            };

            this.Size = r.Next(World.Hook.ObstacleMinSize, World.Hook.ObstacleMaxSize);
        }

        public override void CollisionExecute(WorldBody projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
            {
                if (!bullet.Consumed)
                {
                    bullet.Consumed = true;
                    
                }
            }
        }

        public override bool IsCollision(WorldBody projectedBody)
        {
            var isHit = false;

            if (projectedBody is ShipWeaponBullet bullet)
                return true;

            if (projectedBody is Ship)
                return true;

            if (projectedBody is Obstacle)
                return true;

            return isHit;
        }

        protected override void Update()
        {
            LinearVelocity *= Drag;
            AngularVelocity *= Drag;
        }
    }
}
