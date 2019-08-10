namespace Game.Engine.Core.SystemActors
{
    using System.Linq;

    public abstract class SystemActorBase : IActor
    {
        public World World = null;
        protected long SleepUntil = 0;
        protected int CycleMS = 1000;
        private bool RunningThisStep = false;


        public virtual void CreateDestroy()
        {
            if (RunningThisStep)
                CycleCreateDestroy();
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
            if (World != null && World.Time > SleepUntil)
            {
                RunningThisStep = true;
                CycleThink();

                if (World != null)
                    SleepUntil = World.Time + CycleMS;
            }
            else
                RunningThisStep = false;
        }
    }
}