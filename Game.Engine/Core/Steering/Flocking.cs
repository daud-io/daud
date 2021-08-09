namespace Game.Engine.Core.Steering
{
    using Game.API.Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public static class Flocking
    {
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

                if (relative == Vector2.Zero)
                    return Vector2.Zero;

                var vec = Vector2.Normalize(relative) * distance;

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
                    accumulator += shipOther.LinearVelocity;

            return accumulator / (ships.Count() - 1);
        }

        public static void Flock(Ship ship)
        {
            if (ship.World.Hook.FlockWeight == 0)
                return;

            var fleet = ship.Fleet;
            var hook = ship.World.Hook;

            if (fleet?.Ships == null || fleet.Ships.Count < 2)
                return;

            var shipFlockingVector =
                (hook.FlockCohesion * Flocking.Cohesion(fleet.Ships, ship, hook.FlockCohesionMaximumDistance))
                + (hook.FlockSeparation * Flocking.Separation(fleet.Ships, ship, hook.FlockSeparationMinimumDistance));

            var steeringVector = new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle));
            steeringVector += hook.FlockWeight * shipFlockingVector;

            var newAngle = MathF.Atan2(steeringVector.Y, steeringVector.X);
            if (!float.IsNaN(newAngle))
                ship.Angle = newAngle;
        }
    }
}
