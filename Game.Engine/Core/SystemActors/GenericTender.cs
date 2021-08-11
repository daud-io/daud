namespace Game.Engine.Core.SystemActors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GenericTender<T> : SystemActorBase
        where T : Body
    {
        private readonly List<T> Herd = new List<T>();
        private readonly Func<int> DesiredCount = () => 0;

        public GenericTender(World world, Func<int> desiredCount) : base(world)
        {
            this.DesiredCount = desiredCount;
        }

        private void Add()
        {
            var member = Activator.CreateInstance(typeof(T), World) as T;
            this.Herd.Add(member);
        }

        private void Remove()
        {
            var member = Herd[Herd.Count - 1];
            Herd.Remove(member);
            ((IActor)member).Destroy();
        }

        protected override void CycleThink()
        {
            foreach (var member in Herd.Where(f => !f.Exists).ToList())
                Herd.Remove(member);

            while (Herd.Count < DesiredCount())
                Add();

            while (Herd.Count > DesiredCount())
                Remove();
        }
    }
}