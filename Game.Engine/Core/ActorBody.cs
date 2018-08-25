namespace Game.Engine.Core
{
    using Game.Models;
    using Newtonsoft.Json;

    public abstract class ActorBody : ProjectedBody, IActor
    {
        [JsonIgnore]
        public World World = null;

        public void Deinit()
        {
            World.Actors.Remove(this);
            World.Bodies.Remove(this);
            this.Exists = false;
        }

        public virtual void Init(World world)
        {
            World = world;
            this.ID = world.NextID();
            world.Actors.Add(this);
            world.Bodies.Add(this);

            this.Exists = true;
        }

        public abstract void Step();
    }
}
