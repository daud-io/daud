namespace Game.Engine.Core.SystemActors
{
    using System.Linq;

    public abstract class SystemActorBase : IActor
    {
        public World World = null;
        protected long SleepUntil = 0;
        protected int CycleMS = 1000;

        public virtual void CreateDestroy()
        {
        }

        public virtual void Destroy()
        {
            this.World.Actors.Remove(this);
        }

        public virtual void Init(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);
        }

        protected virtual void CycleThink() { }
        protected virtual void CycleCreateDestroy() { }

        public virtual void Think()
        {
            if (World.Time > SleepUntil)
            {
                CycleThink();

                SleepUntil = World.Time + CycleMS;
            }
        }
    }
}