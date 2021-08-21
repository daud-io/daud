namespace Game.Engine.Core.Weapons
{
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading;
    using Game.Engine.Physics;

    public class ShipWeaponBullet : WorldBody, IShipWeapon
    {
        public Fleet OwnedByFleet { get; set; }
        public long TimeDeath { get; set; }
        public long TimeBirth { get; set; }

        public float ThrustAmount { get; set; }

        public float Drag { get => World.Hook.Drag; }

        public bool Consumed { get; set; }
        private Vector2 Reference = Vector2.Zero;

        public ShipWeaponBullet(World world, Ship ship): base(world)
        {
            this.OwnedByFleet = ship.Fleet;
            Interlocked.Increment(ref World.ProjectileCount);
        }

        protected override void Update()
        {
            Angle = MathF.Atan2(LinearVelocity.Y, LinearVelocity.X);
            AngularVelocity = 0;

            //MaximumSpeed = MathF.Max(LinearVelocity.Length(), MaximumSpeed);

            if (World.Time >= TimeDeath || Consumed)
                Die();
        }

        public virtual void FireFrom(Ship ship, ActorGroup group)
        {
            var r = new Random();
            var bulletOrigin = ship.Position
                + new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle)) * ship.Size;

            var momentum =
                new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle))
                * Vector2.Distance(ship.LinearVelocity, Vector2.Zero);

            this.TimeDeath = World.Time + (long)(World.Hook.BulletLife);
            this.LinearVelocity = momentum;
            this.Position = bulletOrigin;

            if (World.Hook.PrecisionBullets && ship.Fleet != null)
            {
                Vector2 toTarget = Vector2.Zero;
                if (World.Hook.PrecisionBulletsMinimumRange > 0 && ship.Fleet.AimTarget != Vector2.Zero)
                {
                    var minAim = Vector2.Normalize(ship.Fleet.AimTarget) * MathF.Max(ship.Fleet.AimTarget.Length(), World.Hook.PrecisionBulletsMinimumRange);

                    toTarget = (ship.Fleet.FleetCenter + minAim) - ship.Position;
                }
                else
                    toTarget = (ship.Fleet.FleetCenter + ship.Fleet.AimTarget) - ship.Position;

                var noise = 0f;
                if (ship.Fleet.Ships.Count > 1)
                    noise = ((float)r.NextDouble() - 0.5f) * World.Hook.PrecisionBulletsNoise;

                this.Angle = MathF.Atan2(toTarget.Y, toTarget.X)
                    + noise;

                this.AngularVelocity = 0;
            }
            else
            {
                this.Angle = ship.Angle;
                this.AngularVelocity = 0;
            }

            this.Reference = ship.Fleet.FleetVelocity;
            this.Sprite = ship.BulletSprite;
            this.Size = 20;
            this.Color = ship.Color;
            this.ThrustAmount = ship.Fleet.Ships.Count() * ship.Fleet.ShotThrustM + ship.Fleet.ShotThrustB;
            this.TimeBirth = World.Time;
            this.Group = group;

            var thrust = new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * ThrustAmount * 10;

            if (World.Hook.EinsteinCoefficient > 0)
                LinearVelocity = thrust + (World.Hook.EinsteinCoefficient * Reference);
            else
                LinearVelocity = thrust;

        }

        public override void Destroy()
        {
            Interlocked.Decrement(ref World.ProjectileCount);
            base.Destroy();
        }

        bool IShipWeapon.Active()
        {
            return this.Exists;
        }
    }
}
