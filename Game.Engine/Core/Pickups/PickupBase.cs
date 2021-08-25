namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using System;
    using System.Linq;
    using System.Numerics;

    public abstract class PickupBase : WorldBody
    {
        public long TimeDeath { get; set; }
        public Fleet ExcludedFleet { get; set; }
        public bool DontRandomize { get; set; }
        public float Drag { get; set; } = 1.0f;

        public PickupBase(World world): base(world)
        {
            Size = 100;
            this.Mass = 0.001f;
            Sprite = Sprites.seeker_pickup;
            if (!DontRandomize)
                Randomize();
        }

        public virtual void Randomize()
        {
            var r = new Random();
            Position = World.RandomPosition();
            LinearVelocity = new Vector2(0,0);
            AngularVelocity = 0.005f;
        }

        protected abstract void EquipFleet(Fleet fleet);

        public override void CollisionExecute(WorldBody otherBody)
        {
            var ship = otherBody as Ship;
            var fleet = ship?.Fleet;
            if (fleet != null && fleet != ExcludedFleet)
            {
                EquipFleet(fleet);
                Randomize();
            }

            base.CanCollide(otherBody);
        }

        public override CollisionResponse CanCollide(WorldBody otherBody)
        {
            var ship = otherBody as Ship;
            var fleet = ship?.Fleet;

            if (fleet != null && fleet != ExcludedFleet)
                return new CollisionResponse(true, false);

            if (otherBody == null)
                return new CollisionResponse(true, true);
                
            return base.CanCollide(otherBody);
        }

        protected override void Update()
        {

            if (TimeDeath > 0 && TimeDeath < World.Time)
                Die();

            if (World.DistanceOutOfBounds(Position) > 0)
            {
                var speed = LinearVelocity.Length();
                if (Position != Vector2.Zero)
                    LinearVelocity = Vector2.Normalize(Vector2.Zero - Position) * speed;
            }

            if (Drag != 1)
                LinearVelocity *= Drag;

            base.Update();
        }

        public static T FireFrom<T>(Fleet fleet)
            where T : PickupBase
        {

            var pickup = Activator.CreateInstance(typeof(T), fleet.World) as T;
            pickup.Position = fleet.FleetCenter;
            pickup.Angle = MathF.Atan2(fleet.AimTarget.Y, fleet.AimTarget.X);

            pickup.ExcludedFleet = fleet;
            pickup.LinearVelocity = fleet.FleetVelocity;
            pickup.Drag = 0.98f;

            pickup.DontRandomize = true;

            if (fleet.AimTarget != Vector2.Zero)
                pickup.LinearVelocity = Vector2.Normalize(fleet.AimTarget)
                    * ((fleet.Ships.Count() * fleet.ShotThrustM + fleet.ShotThrustB) * 10);

            return pickup;
        }
    }
}
