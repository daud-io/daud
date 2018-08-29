namespace Game.Engine.Core
{
    using Newtonsoft.Json;

    public abstract class ActorBody : ProjectedBody, IActor
    {
        [JsonIgnore]
        public World World = null;

        public virtual void Deinit()
        {
            if (this.Exists)
            {
                World.Actors.Remove(this);
                World.Bodies.Remove(this);
                this.Exists = false;
            }
        }

        public virtual void Init(World world)
        {
            World = world;
            this.ID = world.NextID();
            world.Actors.Add(this);
            world.Bodies.Add(this);

            this.OriginalPosition = this.Position;
            this.DefinitionTime = world.Time;
            this.Project(world.Time);

            this.Exists = true;
        }

        public abstract void Step();
    }
}
