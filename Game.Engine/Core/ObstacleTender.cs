namespace Game.Engine.Core
{
    using System.Collections.Generic;

    public class ObstacleTender : IActor
    {
        private readonly List<Obstacle> Obstacles = new List<Obstacle>();
        private readonly List<Pickup> Pickups = new List<Pickup>();
        private World World = null;

        private void AddObstacle()
        {
            var obstacle = new Obstacle(World);
            this.Obstacles.Add(obstacle);
        }

        private void RemoveObstacle()
        {
            var obstacle = Obstacles[Obstacles.Count - 1];
            Obstacles.Remove(obstacle);
            obstacle.Deinit();
        }

        private void AddPickup()
        {
            var Pickup = new Pickup(World);
            this.Pickups.Add(Pickup);
        }

        private void RemovePickup()
        {
            var Pickup = Pickups[Pickups.Count - 1];
            Pickups.Remove(Pickup);
            Pickup.Deinit();
        }

        public void Step()
        {
            int desiredObstacles = World.Hook.Obstacles;
            int desiredPickups = World.Hook.Pickups;

            while (Obstacles.Count < desiredObstacles)
                AddObstacle();

            while (Obstacles.Count > desiredObstacles)
                RemoveObstacle();

            while (Pickups.Count < desiredPickups)
                AddPickup();

            while (Pickups.Count > desiredPickups)
                RemovePickup();
        }

        public void Init(World world)
        {
            this.World = world;
            this.World.Actors.Add(this);
        }

        public void Deinit()
        {
            this.World.Actors.Remove(this);
        }
    }
}