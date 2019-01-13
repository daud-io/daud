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
                position += momentum * stepSize;
            }

            return position;
        }

        public static Vector2 FiringIntercept(HookComputer hook, Vector2 fromPosition, Vector2 targetPosition, Vector2 targetMomentum, int fleetSize)
        {
            var toTarget = targetPosition - fromPosition;

            var bulletSpeed = hook.ShotThrust(fleetSize) * 10;

            var a = Vector2.Dot(targetMomentum, targetMomentum) - (bulletSpeed * bulletSpeed);
            var b = 2 * Vector2.Dot(targetMomentum, toTarget);
            var c = Vector2.Dot(toTarget, toTarget);

            var p = -b / (2 * a);
            var q = MathF.Sqrt((b * b) - 4 * a * c) / (2 * a);

            var t1 = p - q;
            var t2 = p + q;
            var t = 0f;

            if (t1 > t2 && t2 > 0)
                t = t2;
            else
                t = t1;

            var aimSpot = targetPosition + targetMomentum  * t;

            var bulletPath = aimSpot - fromPosition;
            var timeToImpact = bulletPath.Length() / bulletSpeed;//speed must be in units per second            

            return aimSpot;
        }
    }
}
