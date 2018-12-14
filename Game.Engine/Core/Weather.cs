namespace Game.Engine.Core
{
    using System.Linq;
    
    public class Weather : IActor
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
            
        }
    }
}
