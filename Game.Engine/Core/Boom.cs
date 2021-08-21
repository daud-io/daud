namespace Game.Engine.Core
{
    using Game.API.Common;

    public class Boom : WorldBody
    {
        public float Drag { get; set; }
        private long TimeDeath = 0;

        public static Boom FromShip(Ship ship)
        {
            if (ship.World.Hook.BoomLife > 0)
            {
                var boom = new Boom(ship.World);
                boom.Size = ship.Size;
                boom.Position = ship.Position;
                boom.LinearVelocity = ship.LinearVelocity;

                return boom;
            }
            else
                return null;
        }

        public Boom(World world) : base(world)
        {
            Sprite = Sprites.boom;
            Drag = World.Hook.BoomDrag;
            TimeDeath = World.Time + World.Hook.BoomLife;
        }

        protected override void Update()
        {
            if (TimeDeath > 0 && World.Time > TimeDeath)
                Die();

            LinearVelocity *= Drag;

            base.Update();
        }
    }
}
