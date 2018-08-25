namespace Game.Engine.Core
{
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;

    public class Bullet : ActorBody
    {
        [JsonIgnore]
        public Fleet Owner { get; set; }
        public long TimeDeath { get; set; }
        
        public static void FireFrom(Fleet fleet)
        {
            var world = fleet.World;

            var bullet = new Bullet
            {
                TimeDeath = world.Time + world.Hook.BulletLife,
                Momentum = new Vector2(
                        (float)Math.Cos(fleet.Angle),
                        (float)Math.Sin(fleet.Angle)
                    ) * world.Hook.BulletSpeed,
                Position = fleet.Position,
                Angle = fleet.Angle,
                Owner = fleet,
                Sprite = "bullet",
                Size = 20
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
