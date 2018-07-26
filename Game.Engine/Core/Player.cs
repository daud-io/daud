namespace Game.Engine.Core
{
    using Game.Models;
    using System.Numerics;

    public class Player
    {
        public string Name { get; set; } = null;
        public Vector2 Momentum { get; set; } = new Vector2(0,0);
        public PlayerView View { get; set; } = null;
        public GameObject GameObject { get; set; } = null;

        public void Step(World world)
        {
            View = View ?? new PlayerView();

            View.Time = world.Time;
            View.PlayerCount = world.PlayerCount;

            View.Objects = world.Objects;

            View.Position = GameObject?.Position;
        }
    }
}
