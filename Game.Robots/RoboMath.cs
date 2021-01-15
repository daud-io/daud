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
            HookComputer hookComputer,
            Vector2 position,
            ref Vector2 momentum,
            int fleetSize,
            float angle,
            long ms
        )
        {
            var thrustAmount = hookComputer.ShipThrust(fleetSize);
            var stepSize = hookComputer.Hook.StepTime;

            for (var time = 0; time <= ms; time += stepSize)
            {
                var thrust = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * thrustAmount;
                momentum = (momentum + thrust) * hookComputer.Hook.Drag;
                position += momentum * stepSize;
            }

            return position;
        }

        public static Vector2 FiringIntercept(
            HookComputer hook,
            Vector2 fromPosition,
            Vector2 targetPosition,
            Vector2 targetMomentum,
            int fleetSize
        )
        {
            int timeToImpact = 0;
            return FiringIntercept(hook, fromPosition, targetPosition, targetMomentum, fleetSize, out timeToImpact);
        }

        public static Vector2 FiringIntercept(
            HookComputer hook,
            Vector2 fromPosition,
            Vector2 targetPosition,
            Vector2 targetMomentum,
            int fleetSize,
            out int timeToImpact
        )
        {
            // http://danikgames.com/blog/how-to-intersect-a-moving-target-in-2d/
            var ab = targetPosition - fromPosition;
            var dist = ab.Length();
            var ui = targetMomentum - Vector2.Dot(targetMomentum, ab / dist) * ab / dist;
            var bulletSpeed = hook.ShotThrust(fleetSize) * 10;
            var vj_mag = MathF.Sqrt(MathF.Max(bulletSpeed * bulletSpeed - ui.LengthSquared(), 0f));
            var ret = ab + ui * dist / vj_mag;
            timeToImpact = (int)(ret.Length() / bulletSpeed);
            return ret + fromPosition;
        }
        public static bool MightHit(
            HookComputer hook,
            Fleet shooter,
            Fleet monstrosity,
            float angle
        )
        {
            Vector2 dirN = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            bool hit = false;
            foreach (var firer in shooter.Ships)
            {
                foreach (var other in monstrosity.Ships)
                {
                    var toTarget = other.Position - firer.Position;

                    var bulletSpeed = hook.ShotThrust(shooter.Ships.Count) * 10;
                    var b = toTarget.Length();

                    var c = Vector2.Dot(dirN, toTarget);

                    var q = MathF.Sqrt((b * b) - c * c);


                    var bulletPath = toTarget;
                    var timeToImpact = (int)(bulletPath.Length() / bulletSpeed);//speed must be in units per second            

                    if (timeToImpact < hook.Hook.BulletLife && q < 1000 && c > 0)
                    {
                        hit = true;
                        break;
                    }
                }
            }
            return hit;
        }
        public static Vector2 ProjectClosest(HookComputer hook, Vector2 fromPosition, Vector2 targetPosition, float maxTime, int fleetSize)
        {
            var boostSpeed = hook.Hook.BoostThrust;
            var bulletSpeed = hook.ShotThrust(fleetSize) * 10;
            var path = targetPosition - fromPosition;
            var pLen = path.Length();
            var maxD = bulletSpeed * maxTime + boostSpeed * hook.Hook.BoostDuration;
            // if(maxD>pLen){
            //     Console.Write("Switch");
            // }
            return fromPosition + path * (1.0f / pLen) * MathF.Min(pLen - 10.0f, maxD);

        }
    }
}
