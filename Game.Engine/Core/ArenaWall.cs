namespace Game.Engine.Core
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using Game.Engine.Physics;
    using System;
    using System.Numerics;

    public class ArenaWall : WorldBody
    {
        public enum WhichWall
        {
            North,
            South,
            West,
            East

        }
        private WhichWall Which;

        public ArenaWall(World world, WhichWall which): base(world)
        {
            this.BecomeKinematic();

            size = 0;
            this.Sprite = API.Common.Sprites.ctf_base;
            this.Which = which;
        }

        protected override void DefinePhysicsObject(float size, float mass)
        {
            var shape = new Box(this.World.Hook.WorldSize * 2, 200, 200);
            var position2d = new Vector2(0, this.World.Hook.WorldSize);

            ShapeHandle = World.Simulation.Shapes.Add(shape);
            BodyHandle = World.Simulation.Bodies.Add(BodyDescription.CreateDynamic(
                new Vector3(position2d.X, 0, position2d.Y),
                GetBodyInertia(shape, mass),
                new CollidableDescription(ShapeHandle, 0.1f),
                new BodyActivityDescription(0.00f)
            ));

            ref var worldBodyProperties = ref World.BodyProperties.Allocate(BodyHandle);
            worldBodyProperties = new WorldBodyProperties
            {
                Friction = 0.0f,
            };
        }

        public override void CollisionExecute(WorldBody projectedBody)
        {
            
        }

        public override CollisionResponse CanCollide(WorldBody projectedBody)
        {
            return new CollisionResponse(true, true);
        }

        protected override void Update()
        {
            ref var shape = ref World.Simulation.Shapes.GetShape<Box>(ShapeHandle.Index);
            shape.Width = this.World.Hook.WorldSize * 2;

            switch (Which)
            {
                case WhichWall.South: 
                    Position = new Vector2(0, this.World.Hook.WorldSize + shape.HalfLength);
                    Angle = 0;
                    break;
                case WhichWall.North: 
                    Position = new Vector2(0, -1 * (this.World.Hook.WorldSize + shape.HalfLength));
                    Angle = MathF.PI;
                    break;
                case WhichWall.West: 
                    Position = new Vector2(-1 * (this.World.Hook.WorldSize + shape.HalfLength), 0);
                    Angle = MathF.PI/2;
                    break;
                case WhichWall.East: 
                    Position = new Vector2(this.World.Hook.WorldSize + shape.HalfLength, 0);
                    Angle = MathF.PI * 1.5f;
                    break;

            }

            base.Update();
        }
    }
}
