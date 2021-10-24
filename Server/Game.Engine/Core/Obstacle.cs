namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;
    using System;
    using System.Numerics;

    public class Obstacle : WorldBody
    {
        private Vector2 TargetVelocity = Vector2.Zero;
        protected float Drag = 0.00075f;

        public Obstacle(World world) : base(world)
        {
            var r = new Random();
            Position = World.RandomPosition();
            LinearVelocity = new Vector2(0,0);

            Sprite = Sprites.sportsball;
            Color = "rgba(128,128,128,.2)";
            this.Mass = size;

            this.Group = new Group(world)
            {
                GroupType = GroupTypes.Obstacle,
                ZIndex = 95
            };

            this.Size = r.Next(World.Hook.ObstacleMinSize, World.Hook.ObstacleMaxSize);
        }

        public override void CollisionExecute(WorldBody projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
            {
                if (!bullet.Consumed)
                    bullet.Consumed = true;
            }
        }

        public override CollisionResponse CanCollide(WorldBody projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
                return new CollisionResponse(true, true, frictionCoefficient: 0.3f);

            if (projectedBody is Ship)
                return new CollisionResponse(true, true, frictionCoefficient: 0.3f);

            if (projectedBody is Obstacle)
                return new CollisionResponse(true, true, frictionCoefficient: 0.3f);

            return base.CanCollide(projectedBody);
        }

        protected override void Update(float dt)
        {
            LinearVelocity *= (1f-Drag*dt);
            AngularVelocity *= (1f-Drag*dt);
            base.Update(dt);
        }
    }
}
