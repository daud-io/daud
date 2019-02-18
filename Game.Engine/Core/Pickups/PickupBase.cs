namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using System;
    using System.Numerics;

    public abstract class PickupBase : ActorBody
    {
        public PickupBase()
        {
            Size = 100;
            Sprite = Sprites.seeker_pickup;
            CausesCollisions = true;
        }

        public override void Init(World world)
        {
            World = world;
            Randomize();
            base.Init(world);
        }

        public virtual void Randomize()
        {
            var r = new Random();
            Position = World.RandomPosition();
            Momentum = new Vector2(
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum),
                (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum)
            );

            AngularVelocity = 0.005f;
        }

        protected abstract void EquipFleet(Fleet fleet);

        protected override void Collided(ICollide otherObject)
        {
            var ship = otherObject as Ship;
            var fleet = ship?.Fleet;

            if (fleet != null)
            {
                EquipFleet(fleet);

                Randomize();
            }

            base.Collided(otherObject);
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
