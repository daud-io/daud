using System.Linq;

namespace Game.Engine.Core.SystemActors
{
    public class RoomReset : SystemActorBase
    {
        public bool Reset { get; set; } = false;

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