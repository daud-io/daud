namespace Game.Engine.Core
{
    public class ObstacleTender : IActor
    {
        private World World = null;

        private readonly GenericTender<Obstacle> Obstacles = new GenericTender<Obstacle>();
        private readonly GenericTender<Fish> Fishes = new GenericTender<Fish>();
        private readonly GenericTender<Pickup> Pickups = new GenericTender<Pickup>();
        private readonly GenericTender<Wormhole> Wormholes = new GenericTender<Wormhole>();

        public void Think()
        {
            Obstacles.DesiredCount = World.Hook.Obstacles;
            Fishes.DesiredCount = World.Hook.Fishes;
            Pickups.DesiredCount = World.Hook.Pickups;
            Wormholes.DesiredCount = World.Hook.Wormholes;
        }

        public void Init(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);

            Obstacles.Init(world);
            Fishes.Init(world);
            Pickups.Init(world);
            Wormholes.Init(world);
        }

        public void Destroy()
        {
            this.World.Actors.Remove(this);
            Obstacles.Destroy();
            Fishes.Destroy();
            Pickups.Destroy();
            Wormholes.Destroy();
        }

        public void CreateDestroy()
        {
        }
    }
}