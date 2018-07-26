namespace Game.Engine.Core
{
    using Game.Models;
    using System;
    using System.Numerics;

    public class Player
    {
        public string Name { get; set; } = null;
        public Vector2 Thrust { get; set; } = new Vector2(0,0);
        public PlayerView View { get; set; } = null;
        public GameObject GameObject { get; set; } = null;

        public void Step(World world)
        {
            View = View ?? new PlayerView();

            View.Time = world.Time;
            View.PlayerCount = world.PlayerCount;

            View.Objects = world.Objects;

            View.Position = GameObject?.Position;


            // apply thrust
            int MAX_SPEED = 3;

            GameObject.Angle = (float)Math.Atan2(Thrust.Y, Thrust.X);

            var x = Vector2.Add(GameObject.Momentum, Thrust);
            if (Math.Abs(Vector2.Distance(x, Vector2.Zero)) > MAX_SPEED)
                x = Vector2.Multiply(Vector2.Normalize(x), MAX_SPEED);

            GameObject.Momentum = x;
        }
    }
}
