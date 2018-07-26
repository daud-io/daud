namespace Game.Engine.Core
{
    using Game.Models;
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

            var momentum = GameObject.Momentum;
            var thrust = Thrust;


            var x = Vector2.Add(momentum, thrust);

            int MAX_SPEED = 10;

            if (Vector2.Distance(momentum, Vector2.Zero) > MAX_SPEED)
                x = Vector2.Multiply(Vector2.Normalize(x), MAX_SPEED);

            GameObject.Momentum = x;
        }
    }
}
