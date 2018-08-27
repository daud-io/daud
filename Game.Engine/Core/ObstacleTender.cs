namespace Game.Engine.Core.Actors.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public class ObstacleTender : IActor
    {
        private readonly List<Obstacle> Obstacles = new List<Obstacle>();
        private World World = null;

        private void AddObstacle()
        {
            var r = new Random();

            var obstacle = new Obstacle()
            {
                Position = World.RandomPosition(),
                Momentum = new Vector2(
                    (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum),
                    (float)(r.NextDouble() * 2 * World.Hook.ObstacleMaxMomentum - World.Hook.ObstacleMaxMomentum)
                ),
                Size = r.Next(0, World.Hook.ObstacleMaxSize),
                Sprite = "obstacle",
                Color = "rgba(128,128,128,.2)"
            };

            obstacle.Init(World);
            this.Obstacles.Add(obstacle);
        }

        public void Step()
        {
            int desired = World.Hook.Obstacles;

            while (Obstacles.Count < desired)
                AddObstacle();

            while (Obstacles.Count > desired)
            {
                var obstacle = Obstacles[Obstacles.Count - 1];
                Obstacles.Remove(obstacle);
                obstacle.Deinit();
            }
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