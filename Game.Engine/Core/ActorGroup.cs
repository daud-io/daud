namespace Game.Engine.Core
{
    public class ActorGroup : Group, IActor
    {
        public World World = null;

        public bool PendingDestruction { get; set; } = false;

        public virtual void Destroy()
        {
            if (this.Exists)
            {
                World.Actors.Remove(this);
                World.Groups.Remove(this);
                this.Exists = false;
            }
        }

        public virtual void Init(World world)
        {
            World = world;
            this.ID = world.NextID();
            world.Actors.Add(this);
            world.Groups.Add(this);

            this.Exists = true;
        }

        public virtual void Think()
        {
        }

        public virtual void CreateDestroy()
        {
            if (PendingDestruction)
            {
                PendingDestruction = false;
                Destroy();
            }
        }
    }
}
