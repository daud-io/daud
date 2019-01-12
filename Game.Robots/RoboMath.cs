namespace Game.Robots
{
    using Game.Robots.Models;
    using System;
    using System.Numerics;

    public static class RoboMath
    {
        public static float CalculateDifferenceBetweenAngles(float firstAngle, float secondAngle)
        {
            float difference = secondAngle - firstAngle;
            if (difference < -MathF.PI) difference += 2 * MathF.PI;
            if (difference > MathF.PI) difference -= 2 * MathF.PI;
            return difference;
        }

        public static Vector2 ShipThrustProjection(
            HookComputer hook,
            Vector2 position, 
            Vector2 momentum,
            int fleetSize,
            float angle,
            long ms
        )
        {
            var thrustAmount = hook.ShipThrust(fleetSize);
            var stepSize = hook.StepSize;
            
            for (var time=0; time <= ms; time += stepSize)
            {
                var thrust = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * thrustAmount;
                momentum = (momentum + thrust) * hook.Drag;
                position += momentum * hook.StepSize;
            }

            return position;
        }
    }
}
