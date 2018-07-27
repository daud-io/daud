namespace Game.Engine.Core
{
    using Game.Models;
    using System.Numerics;

    public class Bullet
    {
        public GameObject GameObject { get; set; } = null;
        public long EndOfLife { get; set; } = 0;

        public Bullet(World world, Vector2 position, Vector2 momentum, float angle)
        {
            this.GameObject = new GameObject
            {
                Position = position,
                Momentum = momentum,
                Sprite = "bullet",
                Angle = angle
            };
            world.Objects.Add(this.GameObject);
            world.Bullets.Add(this);

            EndOfLife = world.Time + 3000;
        }

        public void Step(World world)
        {
            if (EndOfLife < world.Time)
            {
                world.Bullets.Remove(this);
                world.Objects.Remove(this.GameObject);
            }
        }
    }
}
