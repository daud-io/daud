namespace Game.Engine.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public class ObstacleTender : IActor
    {
        private readonly List<Obstacle> Obstacles = new List<Obstacle>();
        private readonly List<Pickup> Pickups = new List<Pickup>();
        private readonly List<Fish> Fishes = new List<Fish>();
        private World World = null;

        private void AddObstacle()
        {
            var obstacle = new Obstacle();
            obstacle.Init(World);
            this.Obstacles.Add(obstacle);
        }

        private void RemoveObstacle()
        {
            var obstacle = Obstacles[Obstacles.Count - 1];
            Obstacles.Remove(obstacle);
            obstacle.Destroy();
        }

        private void AddPickup()
        {
            var Pickup = new Pickup(World);
            Pickup.Init(World);
            this.Pickups.Add(Pickup);
        }

        private void RemovePickup()
        {
            var Pickup = Pickups[Pickups.Count - 1];
            Pickups.Remove(Pickup);
            Pickup.Destroy();
        }

        private void AddFish()
        {
            var fish = new Fish(World);
            this.Fishes.Add(fish);
        }

        private void RemoveFish()
        {
            var fish = Fishes[Fishes.Count - 1];
            Fishes.Remove(fish);
            fish.Destroy();
        }

        public void Think()
        {
        }

        public void CreateDestroy()
        {
            int desiredObstacles = World.Hook.Obstacles;
            int desiredPickups = World.Hook.Pickups;
            int desiredFishes = World.Hook.Fishes;

            while (Obstacles.Count < desiredObstacles)
                AddObstacle();

            while (Obstacles.Count > desiredObstacles)
                RemoveObstacle();

            while (Pickups.Count < desiredPickups)
                AddPickup();

            while (Pickups.Count > desiredPickups)
                RemovePickup();

            foreach (var fish in Fishes.Where(f => !f.Exists).ToList())
                Fishes.Remove(fish);

            while (Fishes.Count < desiredFishes)
                AddFish();

            while (Fishes.Count > desiredFishes)
                RemoveFish();
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