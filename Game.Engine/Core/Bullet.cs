namespace Game.Engine.Core
{
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;

    public class Bullet : ActorBody
    {
        [JsonIgnore]
        public Fleet OwnedByFleet { get; set; }
        public long TimeDeath { get; set; }
        
        public static void FireFrom(Ship ship)
        {
            var world = ship.World;

            var shotSpeed = ship.Fleet.ShotSpeedM
                * ship.Fleet.Ships.Count()
                + ship.Fleet.ShotSpeedB;

            var bullet = new Bullet
            {
                TimeDeath = world.Time + world.Hook.BulletLife,
                Momentum = new Vector2(
                        (float)Math.Cos(ship.Angle),
                        (float)Math.Sin(ship.Angle)
                    ) * shotSpeed,
                Position = ship.Position,
                Angle = ship.Angle,
                OwnedByFleet = ship.Fleet,
                Sprite = "bullet",
                Size = 20,
                Color = ship.Color
            };
            bullet.Init(world);
        }

        public override void Step()
        {
            var collisionSet = World.BodiesNear(this.Position, this.Size, offsetSize: true);
            if (collisionSet.Any())
            {
                foreach (var hit in collisionSet.OfType<ICollide>()
                    .Where(c => c.IsCollision(this))
                    .ToList())
                {
                    hit.CollisionExecute(this);

                    TimeDeath = World.Time;
                }
            }

            if (World.Time >= TimeDeath)
                Deinit();
        }
    }
}
