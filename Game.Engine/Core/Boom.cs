namespace Game.Engine.Core
{
    using Game.API.Common;

    public class Boom : ActorBody
    {
        public float Drag { get; set; }
        private long TimeDeath = 0;

        public override void Init(World world)
        {
            base.Init(world);

            Sprite = Sprites.boom;
            Drag = World.Hook.BoomDrag;
            TimeDeath = World.Time + World.Hook.BoomLife;
        }

        public static Boom FromShip(Ship ship)
        {
            var boom = new Boom();
            boom.Init(ship.World);
            boom.Size = ship.Size;
            boom.Position = ship.Position;
            boom.Momentum = ship.Momentum;

            return boom;
        }

        public override void Think()
        {
            base.Think();

            if (TimeDeath > 0 && World.Time > TimeDeath)
                PendingDestruction = true;

            Momentum *= Drag;
        }
    }
}
