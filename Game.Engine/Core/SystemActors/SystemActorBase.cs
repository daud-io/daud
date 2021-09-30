namespace Game.Engine.Core.SystemActors
{
    public abstract class SystemActorBase : IActor
    {
        public World World = null;
        protected long SleepUntil = 0;
        protected int CycleMS = 1000;
        public SystemActorBase(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);
        }

        public virtual void Destroy()
        {
            this.World.Actors.Remove(this);
        }

        protected virtual void CycleThink() { }
        public virtual void Think(float dt)
        {
            if (World != null && World.Time > SleepUntil)
            {
                CycleThink();

                if (World != null)
                    SleepUntil = World.Time + CycleMS;
            }
        }

        public void Cleanup()
        {
        }
    }
}