namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;
    using System;
    using System.Numerics;

    public class Obstacle : WorldBody
    {
        private Vector2 TargetVelocity = Vector2.Zero;
        //protected float Drag = 0.97f;
        protected float Drag = 1.0f;

        public Obstacle(World world) : base(world)
        {
            var r = new Random();
            Position = World.RandomPosition();
            LinearVelocity = new Vector2(0,0);

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
                    bullet.Consumed = true;
            }
        }

        public override CollisionResponse CanCollide(WorldBody projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
                return new CollisionResponse(true, true);

            if (projectedBody is Ship)
                return new CollisionResponse(true, true);

            if (projectedBody is Obstacle)
                return new CollisionResponse(true, true);

            return base.CanCollide(projectedBody);
        }

        protected override void Update()
        {
            LinearVelocity *= Drag;
            AngularVelocity *= Drag;
            base.Update();
        }
    }
}
