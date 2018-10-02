namespace Game.Engine.Core.Steering
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public static class Flocking
    {

        public static Vector2 FleetCenterNaive(IEnumerable<Ship> ships, Ship except = null)
        {
            Vector2 accumlator = Vector2.Zero;

            foreach (var ship in ships.Where(s => s != except))
                accumlator += ship.Position;

            accumlator /= ships.Count();

            return accumlator;
        }

        public static Vector2 Cohesion(IEnumerable<Ship> ships, Ship ship, int maximumDistance)
        {
            var exclusiveCenter = Vector2.Zero;
            int shipsIncluded = 0;
            foreach (var shipOther in ships)
            {
                if (shipOther != ship)
                {
                    var distance = Vector2.Distance(ship.Position, shipOther.Position);
                    if (distance < maximumDistance)
                    {
                        exclusiveCenter += shipOther.Position;
                        shipsIncluded++;
                    }
                }
            }

            if (shipsIncluded > 0)
            {
                exclusiveCenter /= shipsIncluded;
                var relative = exclusiveCenter - ship.Position;
                var distance = Vector2.Distance(ship.Position, exclusiveCenter);

                var vec = Vector2.Normalize(relative) * distance;

                if (float.IsNaN(vec.X))
                    throw new System.Exception("Bad position");

                return vec;
            }
            else
                return Vector2.Zero;
        }

        public static Vector2 Separation(IEnumerable<Ship> ships, Ship ship, int minimumDistance)
        {
            var accumulator = Vector2.Zero;
            foreach (var shipOther in ships)
            {
                if (shipOther != ship)
                {
                    var distance = Vector2.Distance(ship.Position, shipOther.Position);
                    if (distance < minimumDistance)
                    {
                        if (distance < 1)
                            distance = 1;

                        accumulator += (ship.Position - shipOther.Position) / (distance * distance);
                        //accumulator -= (shipOther.Position - ship.Position);
                    }
                }
            }

            return accumulator;
        }

        public static Vector2 Alignment(IEnumerable<Ship> ships, Ship ship)
        {
            var accumulator = Vector2.Zero;
            foreach (var shipOther in ships)
                if (shipOther != ship)
                    accumulator += shipOther.Momentum;

            return accumulator / (ships.Count() - 1);
        }
    }
}
