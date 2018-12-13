using System.Linq;

namespace Game.Engine.Core
{
    public class Advertisement : IActor
    {
        private World World = null;
        public void CreateDestroy()
        {
        }

        public void Destroy()
        {
            this.World.Actors.Remove(this);
        }

        public void Init(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);
        }

        public void Think()
        {
            World.AdvertisedPlayerCount = Player.GetWorldPlayers(World)
                .Where(p => p.IsAlive || p.IsStillPlaying)
                .Where(p => !(p is Robot))
                .Count();
        }
    }
}
