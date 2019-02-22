namespace Game.Engine.Core.SystemActors
{
    using Game.API.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class Sumo : IActor
    {
        private World World = null;
        private RingBody Ring = null;

        void IActor.CreateDestroy()
        {
            if (World.Hook.SumoMode && Ring == null)
            {
                Ring = new Sumo.RingBody(new Vector2(0, 0), World.Hook.SumoRingSize);
                Ring.Init(World);

                World.FleetSpawnPositionGenerator = this.FleetSpawnPosition;
            }

            if (!World.Hook.SumoMode && Ring != null)
            {
                Ring.Destroy();
                World.FleetSpawnPositionGenerator = null;
            }
        }

        void IActor.Destroy()
        {
            if (Ring != null)
                Ring.Destroy();

            World.Actors.Remove(this);
        }

        void IActor.Init(World world)
        {
            World = world;
            World.Actors.Add(this);
        }

        public Vector2 FleetSpawnPosition(Fleet fleet)
        {
            const int POINTS_TO_TEST = 50;
            const int MAXIMUM_SEARCH_SIZE = 4000;

            var points = new List<Vector2>();
            int failsafe = 10000;

            while (points.Count < POINTS_TO_TEST)
            {
                var position = World.RandomPosition();
                if (Vector2.Distance(position, Ring.Position) < Ring.Size * 0.8f)
                    points.Add(position);

                if (failsafe-- < 0)
                    throw new Exception("Cannot find qualifying location in Sumo Spawn");
            }

            return points.Select(p =>
                {
                    var closeBodies = World.BodiesNear(p, MAXIMUM_SEARCH_SIZE)
                            .OfType<Ship>();
                    return new
                    {
                        Closest = closeBodies.Any()
                            ? closeBodies.Min(s => Vector2.Distance(s.Position, p))
                            : MAXIMUM_SEARCH_SIZE,
                        Point = p
                    };
                })
                .OrderByDescending(location => location.Closest)
                .First().Point;
        }

        void IActor.Think()
        {
            if (Ring != null)
                foreach (var player in Player.GetWorldPlayers(World))
                {
                    if ((player?.Fleet?.Ships?.Count ?? 0) > 0)
                    {
                        var fleet = player.Fleet;
                        foreach (var ship in fleet.Ships.ToList())
                            if (Vector2.Distance(ship.Position, Ring.Position) > Ring.Size)
                                fleet.AbandonShip(ship);
                    }
                }
        }

        private class RingBody : ActorBody
        {
            private const float SPEED_SPINNING = 0.001f;

            public RingBody(Vector2 position, int size)
            {
                this.Position = position;
                this.Sprite = Sprites.ctf_base;
                this.AngularVelocity = SPEED_SPINNING;
                this.Size = size;
            }
        }
    }
}
