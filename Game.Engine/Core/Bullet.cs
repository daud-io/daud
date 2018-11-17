namespace Game.Engine.Core
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class Bullet : ActorBody
    {
        public Fleet OwnedByFleet { get; set; }
        public long TimeDeath { get; set; }

        public bool Seeker { get; set; } = false;

        public float ThrustAmount { get; set; }
        public float ThrustAngle { get; set; }

        public float Drag { get => World.Hook.Drag; }

        public bool Consumed { get; set; }

        public static Bullet FireFrom(Ship ship)
        {
            var world = ship.World;

            var bullet = new Bullet
            {
                TimeDeath = world.Time + world.Hook.BulletLife,
                Momentum = new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle)) * Vector2.Distance(ship.Momentum, Vector2.Zero),
                Position = ship.Position,
                Angle = ship.Angle,
                OwnedByFleet = ship.Fleet,
                Sprite = ship.Fleet.Pickup?.BulletSprite ?? ship.Fleet.BulletSprite,
                Size = ship.Fleet.Pickup?.Size ?? 20,
                Color = ship.Color,
                Seeker = ship.Fleet.Pickup != null,
                ThrustAmount = ship.Fleet.Ships.Count() * ship.Fleet.ShotThrustM + ship.Fleet.ShotThrustB
            };

            return bullet;
        }

        public override void Think()
        {
            base.Think();

            if (Seeker)
            {
                var targets = World.BodiesNear(this.Position, World.Hook.SeekerRange, offsetSize: true);

                var target = targets
                    .OfType<Ship>()
                    .Where(s => s.Fleet != OwnedByFleet)
                    .Where(s => s.Fleet != null)
                    .OrderBy(s => Vector2.Distance(s.Position, Position))
                    .FirstOrDefault();

                if (target != null)
                {
                    var delta = target.Position - Position;
                    ThrustAngle = MathF.Atan2(delta.Y, delta.X);

                    Angle = MathF.Atan2(Momentum.Y, Momentum.X);
                }
                else
                    ThrustAngle = Angle;

                var thrust = new Vector2(MathF.Cos(ThrustAngle), MathF.Sin(ThrustAngle)) * ThrustAmount * World.Hook.SeekerThrustMultiplier;
                Momentum = (Momentum + thrust) * Drag;

            }
            else
            {
                ThrustAngle = Angle;
                var thrust = new Vector2(MathF.Cos(ThrustAngle), MathF.Sin(ThrustAngle)) * ThrustAmount*10;
                Momentum = thrust;
            }

            if (World.Time >= TimeDeath)
                PendingDestruction = true;
        }

        protected override void Collided(ICollide otherObject)
        {
            TimeDeath = World.Time;
        }
    }
}
