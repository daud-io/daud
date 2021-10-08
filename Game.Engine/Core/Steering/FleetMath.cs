namespace Game.Engine.Core.Steering
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public static class FleetMath
    {
        public static Vector2 FleetCenterNaive(IEnumerable<Ship> ships, Ship except = null)
        {
            Vector2 accumlator = Vector2.Zero;
            int count = 0;
            foreach (var ship in ships.Where(s => s != except))
            {
                accumlator += ship.Position;
                count++;
            }

            if (count > 0)
                accumlator /= count;

            return accumlator;
        }

        public static Vector2 FleetVelocity(IEnumerable<Ship> ships, Ship except = null)
        {
            Vector2 accumlator = Vector2.Zero;
            int count = 0;
            foreach (var ship in ships.Where(s => s != except))
            {
                accumlator += ship.LinearVelocity;
                count++;
            }

            if (count > 0)
                accumlator /= count;

            return accumlator;
        }
    }
}
