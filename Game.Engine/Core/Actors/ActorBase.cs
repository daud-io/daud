namespace Game.Engine.Core.Actors
{
    public abstract class ActorBase
    {
        protected World world = null;
        protected bool Deinitialized = false;

        public virtual void Init(World world)
        {
            this.world = world;
            this.world.Actors.Add(this);
        }

        public virtual void Deinit()
        {
            Deinitialized = true;
            this.world.Actors.Remove(this);
        }

        public virtual void PreStep()
        {

        }

        public virtual void Step()
        {

        }

        public virtual void PostStep()
        {

        }
    }
}
