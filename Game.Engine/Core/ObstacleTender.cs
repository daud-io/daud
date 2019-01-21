using Game.Engine.Core.Pickups;

namespace Game.Engine.Core
{
    public class ObstacleTender : IActor
    {
        private World World = null;

        private readonly GenericTender<Obstacle> Obstacles = new GenericTender<Obstacle>();
        private readonly GenericTender<Fish> Fishes = new GenericTender<Fish>();
        private readonly GenericTender<PickupSeeker> PickupSeekers = new GenericTender<PickupSeeker>();
        private readonly GenericTender<Wormhole> Wormholes = new GenericTender<Wormhole>();

        public void Think()
        {
            Obstacles.DesiredCount = World.Hook.Obstacles;
            Fishes.DesiredCount = World.Hook.Fishes;
            PickupSeekers.DesiredCount = World.Hook.PickupSeekers;
            Wormholes.DesiredCount = World.Hook.Wormholes;
        }

        public void Init(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);

            Obstacles.Init(world);
            Fishes.Init(world);
            PickupSeekers.Init(world);
            Wormholes.Init(world);
        }

        public void Destroy()
        {
            this.World.Actors.Remove(this);
            Obstacles.Destroy();
            Fishes.Destroy();
            PickupSeekers.Destroy();
            Wormholes.Destroy();
        }

        public void CreateDestroy()
        {
        }
    }
}