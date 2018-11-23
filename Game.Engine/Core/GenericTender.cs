namespace Game.Engine.Core
{
    using System.Collections.Generic;

    public class GenericTender<T> : IActor
        where T : class, IActor, new()
    {
        private readonly List<T> Herd = new List<T>();
        private World World = null;

        public int DesiredCount = 0;

        private void Add()
        {
            var member = new T();
            member.Init(World);
            this.Herd.Add(member);
        }

        private void Remove()
        {
            var member = Herd[Herd.Count - 1];
            Herd.Remove(member);
            member.Destroy();
        }

        public void Think()
        {
        }

        public void CreateDestroy()
        {
            while (Herd.Count < DesiredCount)
                Add();

            while (Herd.Count > DesiredCount)
                Remove();
        }

        public void Init(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);
        }

        public void Destroy()
        {
            this.World.Actors.Remove(this);
        }
    }
}