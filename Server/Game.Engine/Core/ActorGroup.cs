namespace Game.Engine.Core
{
    public class ActorGroup : Group, IActor
    {
        public bool PendingDestruction { get; set; } = false;

        public ActorGroup(World world): base(world)
        {
            this.ID = world.GenerateObjectID();
            World.Actors.Add(this);
            World.Groups.Add(this);
            Exists = true;
        }

        public virtual void Destroy()
        {
            if (this.Exists)
            {
                World.Actors.Remove(this);
                World.Groups.Remove(this);
                this.Exists = false;
            }
        }

        public virtual void Think(float dt)
        {
        }

        public void Cleanup()
        {
            if (PendingDestruction)
                Destroy();
        }
    }
}
