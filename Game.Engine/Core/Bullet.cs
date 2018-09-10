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

        public static void FireFrom(Ship ship)
        {
            var world = ship.World;

            var bullet = new Bullet
            {
                TimeDeath = world.Time + world.Hook.BulletLife,
                Momentum = new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle)) * Vector2.Distance(ship.Momentum, Vector2.Zero),
                Position = ship.Position,
                Angle = ship.Angle,
                OwnedByFleet = ship.Fleet,
                Sprite = ship.Fleet.Pickup?.BulletSprite ?? "bullet_" + ship.Fleet.Color,
                Size = ship.Fleet.Pickup?.Size ?? 20,
                Color = ship.Color,
                Seeker = ship.Fleet.Pickup != null,
                ThrustAmount = ship.Fleet.Ships.Count() * ship.Fleet.ShotThrustM + ship.Fleet.ShotThrustB
            };
            bullet.Init(world);
        }

        public override void Step()
        {
            base.Step();

            if (Seeker)
            {
                var targets = World.BodiesNear(this.Position, World.Hook.SeekerRange, offsetSize: true);

                var target = targets
                    .OfType<Ship>()
                    .Where(s => s.Fleet != OwnedByFleet)
                    .OrderBy(s => Vector2.Distance(s.Position, Position))
                    .FirstOrDefault();
                if (target != null)
                {
                    var delta = target.Position - Position;
                    ThrustAngle = MathF.Atan2(delta.Y, delta.X);

                    Angle = MathF.Atan2(Momentum.Y, Momentum.X);
                }
            }
            else
            {
                ThrustAngle = Angle;
            }


            var thrust = new Vector2(MathF.Cos(ThrustAngle), MathF.Sin(ThrustAngle)) * ThrustAmount;
            Momentum = (Momentum + thrust) * Drag;


            if (World.Time >= TimeDeath)
                Deinit();
        }

        protected override void Collided(ICollide otherObject)
        {
            TimeDeath = World.Time;

        }
    }
}
