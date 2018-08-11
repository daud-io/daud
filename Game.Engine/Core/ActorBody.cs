namespace Game.Engine.Core
{
    using Game.Models;

    public abstract class ActorBody : ProjectedBody, IActor
    {
        public World World = null;

        public void Deinit()
        {
            World.Actors.Remove(this);
            World.Bodies.Remove(this);
            this.Exists = false;
        }

        public void Init(World world)
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
