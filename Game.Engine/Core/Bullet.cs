namespace Game.Engine.Core
{
    using Game.Models;
    using System.Numerics;

    public class Bullet
    {
        public GameObject GameObject { get; set; } = null;
        public long EndOfLife { get; set; } = 0;
        public Player Owner { get; set; } = null;

        public Bullet(World world, Vector2 position, Vector2 momentum, float angle)
        {
            this.GameObject = new GameObject
            {
                Position = position,
                Momentum = momentum,
                Sprite = "bullet",
                Angle = angle,
                ObjectType = "bullet"
            };
            world.Objects.Add(this.GameObject);
            world.Bullets.Add(this);

            EndOfLife = world.Time + 2000;
        }

        public void Step(World world)
        {
            foreach (var obj in world.Objects)
            {
                if (obj != GameObject)
                {

                    if (obj != this.Owner.GameObject && obj.ObjectType != "bullet")
                    {
                        int COLLISON_DISTANCE = 100;
                        if (Vector2.Distance(obj.Position, GameObject.Position) < COLLISON_DISTANCE)
                        {
                            this.Owner.Score++;
                            EndOfLife = world.Time;
                        }
                    }

                }
            }

            if (EndOfLife <= world.Time)
            {
                world.Bullets.Remove(this);
                world.Objects.Remove(this.GameObject);
            }
        }
    }
}
