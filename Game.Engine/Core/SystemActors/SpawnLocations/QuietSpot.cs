namespace Game.Engine.Core.SystemActors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public static class QuietSpot
    {
        public static Vector2 GeneratorQuietSpot(Fleet fleet)
        {
            const int POINTS_TO_TEST = 50;
            const int MAXIMUM_SEARCH_SIZE = 10000;

            var points = new List<Vector2>();

            for (var i = 0; i < POINTS_TO_TEST; i++)
                points.Add(fleet.World.RandomPosition());

            return points.Select(p =>
                {
                    var closeBodies = fleet.World.BodiesNear(p, MAXIMUM_SEARCH_SIZE)
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
    }
}