namespace Game.Engine.Core.Steering
{
    using System;
    using System.Numerics;

    public static class Snaking
    {
        public static void Snake(Ship ship)
        {
            var hook = ship.World.Hook;
            var fleet = ship.Fleet;

            if (hook.SnakeWeight == 0)
                return;
            if (fleet == null || fleet.Ships.Count < 2)
                return;

            var shipIndex = fleet.Ships.IndexOf(ship);
            if (shipIndex > 0)
            {
                ship.Size = 70;
                var steeringVector = new Vector2(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle));
                steeringVector += (fleet.Ships[shipIndex - 1].Position - ship.Position) * hook.SnakeWeight;
                ship.Angle = MathF.Atan2(steeringVector.Y, steeringVector.X);
            }
            else
                ship.Size = 100;
        }
    }
}
