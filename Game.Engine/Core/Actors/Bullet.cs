namespace Game.Engine.Core.Actors
{
    using Game.Models;
    using System.Linq;
    using System.Numerics;

    public class Bullet : ActorBase
    {
        public GameObject GameObject { get; set; } = null;
        public long EndOfLife { get; set; } = 0;
        public Player Owner { get; set; } = null;

        public Bullet(Vector2 position, Vector2 momentum, float angle)
        {
            this.GameObject = new GameObject
            {
                Position = position,
                Momentum = momentum,
                Sprite = "bullet",
                Angle = angle,
                ObjectType = "bullet"
            };
        }

        public override void Init(World world)
        {
            base.Init(world);

            EndOfLife = world.Time + world.Hook.BulletLife;
            world.Objects.Add(this.GameObject);
        }

        public override void Deinit()
        {
            world.Objects.Remove(this.GameObject);
            base.Deinit();
        }

        public override void Step()
        {
            foreach (var obj in world.Objects.ToList())
            {
                if (obj != GameObject)
                {
                    if (obj != this.Owner.GameObject && obj.ObjectType != "bullet")
                    {

                        int COLLISON_DISTANCE = 100;
                        if (Vector2.Distance(obj.Position, GameObject.Position) < COLLISON_DISTANCE)
                        {

                            var player = world.Players.FirstOrDefault(p => p.GameObject == obj);
                            if (player != null)
                                player.Hit(this);

                            this.Owner.Score++;
                            EndOfLife = world.Time;
                        }
                    }

                }
            }

            if (EndOfLife <= world.Time)
                Deinit();
        }
    }
}
