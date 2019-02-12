namespace HelloMeatbags
{
    using System;
    using System.Numerics;
    using System.Threading.Tasks;
    using Game.Robots;

    class HelloMeatbagsRobot : Robot
    {
        protected override Task AliveAsync()
        {
            Console.WriteLine($"Alive: X:{this.Position.X} Y:{this.Position.Y}");

            this.SteerPointAbsolute(new Vector2(0, 0));

            return base.AliveAsync();
        }

        protected override Task OnDeathAsync()
        {
            Console.WriteLine("Oh snap, I'm dead.");

            return base.OnDeathAsync();
        }

        protected override Task OnSpawnAsync()
        {
            Console.WriteLine("Hooray, I'm alive!");

            return base.OnSpawnAsync();
        }
    }
}
