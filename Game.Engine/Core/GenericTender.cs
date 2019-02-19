namespace Game.Engine.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GenericTender<T> : IActor
        where T : ActorBody, new()
    {
        private readonly List<T> Herd = new List<T>();
        private World World = null;
        private readonly Func<int> DesiredCount = () => 0;

        public GenericTender(Func<int> desiredCount)
        {
            this.DesiredCount = desiredCount;
        }

        private void Add()
        {
            var member = new T();
            ((IActor)member).Init(World);
            this.Herd.Add(member);

            if (member is ILifeCycle canSpawn)
                canSpawn.Spawn();
        }

        private void Remove()
        {
            var member = Herd[Herd.Count - 1];
            Herd.Remove(member);

            if (member is ILifeCycle canDie)
                canDie.Die();
            else
                ((IActor)member).Destroy();
        }

        public void Think()
        {
        }

        public void CreateDestroy()
        {
            foreach (var member in Herd.Where(f => !f.Exists).ToList())
                Herd.Remove(member);

            while (Herd.Count < DesiredCount())
                Add();

            while (Herd.Count > DesiredCount())
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