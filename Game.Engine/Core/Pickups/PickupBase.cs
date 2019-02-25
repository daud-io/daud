namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using System;
    using System.Linq;
    using System.Numerics;

    public abstract class PickupBase : ActorBody
    {
        public long TimeDeath { get; set; }
        public Fleet ExcludedFleet { get; set; }
        public bool DontRandomize { get; set; }
        public float Drag { get; set; } = 1.0f;

        public PickupBase()
        {
            Size = 100;
            Sprite = Sprites.seeker_pickup;
            CausesCollisions = true;
        }

        public override void Init(World world)
        {
            World = world;
            if (!DontRandomize)
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

            if (fleet != null && fleet != ExcludedFleet)
            {
                EquipFleet(fleet);

                Randomize();
            }

            base.Collided(otherObject);
        }

        public override void Think()
        {
            base.Think();


            if (TimeDeath > 0 && TimeDeath < World.Time)
                this.PendingDestruction = true;

            if (World.DistanceOutOfBounds(Position) > 0)
            {
                var speed = Momentum.Length();
                if (Position != Vector2.Zero)
                    Momentum = Vector2.Normalize(Vector2.Zero - Position) * speed;
            }

            if (Drag != 1)
                Momentum *= Drag;
        }

        public static T FireFrom<T>(Fleet fleet)
            where T: PickupBase, new()
        {

            var pickup = new T
            {
                World = fleet.World,
                Position = fleet.FleetCenter,
                Angle = MathF.Atan2(fleet.AimTarget.Y, fleet.AimTarget.X),

                ExcludedFleet = fleet,
                Momentum = fleet.FleetMomentum,
                Drag = 0.98f,


                DontRandomize = true
            };

            if (fleet.AimTarget != Vector2.Zero)
                pickup.Momentum = Vector2.Normalize(fleet.AimTarget)
                    * ((fleet.Ships.Count() * fleet.ShotThrustM + fleet.ShotThrustB) * 10);

            pickup.Init(fleet.World);

            return pickup;
        }

    }
}
