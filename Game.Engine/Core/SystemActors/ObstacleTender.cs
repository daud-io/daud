namespace Game.Engine.Core.SystemActors
{
    using Game.Engine.Core.Pickups;
    using System.Collections.Generic;

    public class ObstacleTender : IActor
    {
        private World World = null;

        private readonly List<IActor> Flock = new List<IActor>();

        public void Think()
        {
        }

        public void Init(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);

            Flock.Add(new GenericTender<Obstacle>(() => World.Hook.Obstacles));
            Flock.Add(new GenericTender<Fish>(() => World.Hook.Fishes));
            Flock.Add(new GenericTender<PickupSeeker>(() => World.Hook.PickupSeekers));
            Flock.Add(new GenericTender<PickupShield>(() => World.Hook.PickupShields));
            Flock.Add(new GenericTender<Wormhole>(() => World.Hook.Wormholes));
            Flock.Add(new GenericTender<PickupRobotGun>(() => World.Hook.PickupRobotGuns));


            foreach (var element in Flock)
                element.Init(world);
        }

        public void Destroy()
        {
            this.World.Actors.Remove(this);

            foreach (var element in Flock)
                element.Destroy();
        }

        public void CreateDestroy()
        {
        }
    }
}