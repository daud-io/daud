namespace Game.Engine.Core.SystemActors
{
    using Game.API.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class Sumo : SystemActorBase
    {
        private RingBody Ring = null;

        public Sumo(World world): base(world)
        {
            
        }

        public override void Destroy()
        {
            if (Ring != null)
                Ring.Destroy();
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

        protected override void CycleThink()
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

            if (World.Hook.SumoMode && Ring == null)
            {
                Ring = new Sumo.RingBody(World, new Vector2(0, 0), World.Hook.SumoRingSize);

                World.GetActor<SpawnLocationsActor>().GeneratorAdd("sumo", this.FleetSpawnPosition);
                World.Hook.SpawnLocationMode = "sumo";
            }

            if (!World.Hook.SumoMode && Ring != null)
            {
                Ring.Destroy();
                World.FleetSpawnPositionGenerator = null;
            }                
        }

        private class RingBody : WorldBody
        {
            private const float SPEED_SPINNING = 0.001f;

            public RingBody(World world, Vector2 position, int size): base(world)
            {
                this.Position = position;
                this.Sprite = Sprites.ctf_base;
                this.AngularVelocity = SPEED_SPINNING;
                this.Size = size;
            }
        }
    }
}
