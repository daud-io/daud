namespace Game.Engine.Core.SystemActors
{
    using System.Linq;

    public class RoomReset : SystemActorBase
    {
        public bool Reset { get; set; } = false;

        public RoomReset(World world): base(world)
        {
        }

        protected override void CycleThink()
        {
            if (Reset)
            {
                foreach (var ship in World.Bodies.OfType<Ship>())
                    ship.Die(null, null, null);

                Reset = false;
            }
        }
    }
}