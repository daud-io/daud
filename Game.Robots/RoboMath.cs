namespace Game.Robots
{
    using Game.Robots.Models;

    using System.Collections.Generic;
    using System;
    using System.Numerics;
    using Campy;
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

            var aimSpot = targetPosition + targetMomentum * t;

            var bulletPath = aimSpot - fromPosition;
            timeToImpact = (int)(bulletPath.Length() / bulletSpeed);//speed must be in units per second            

            if (timeToImpact > hook.Hook.BulletLife)
                timeToImpact = int.MaxValue;

            return aimSpot;
        }
        public static (Vector2[], int[]) FiringIntercepts(
            HookComputer hook,
            Vector2[] fromPosition,
            Vector2[] targetPosition,
            Vector2[] targetMomentum,
            int[] fleetSize
        )
        {
            int n = fleetSize.Length;
            Vector2[] toTarget = new Vector2[n];
            Vector2[] aimSpot = new Vector2[n];
            int[] timeToImpact = new int[n];
            float[] bulletSpeed = new float[n];
            Campy.Parallel.For(n, i => { toTarget[i] = targetPosition[i] - fromPosition[i]; });
            Campy.Parallel.For(n, i => { bulletSpeed[i] = hook.ShotThrust(fleetSize[i]) * 10; });




            //Vector2.Dot(targetMomentum, targetMomentum) - (bulletSpeed * bulletSpeed);

            Campy.Parallel.For(n, i =>
            {
                float a = Vector2.Dot(targetMomentum[i], targetMomentum[i]) - (bulletSpeed[i] * bulletSpeed[i]);
                var b = 2 * Vector2.Dot(targetMomentum[i], toTarget[i]);
                var c = Vector2.Dot(toTarget[i], toTarget[i]);

                var p = -b / (2 * a);
                var q = MathF.Sqrt((b * b) - 4 * a * c) / (2 * a);

                var t1 = p - q;
                var t2 = p + q;
                var t = 0f;

                if (t1 > t2 && t2 > 0)
                    t = t2;
                else
                    t = t1;

                aimSpot[i] = targetPosition[i] + targetMomentum[i] * t;

                var bulletPath = aimSpot[i] - fromPosition[i];
                timeToImpact[i] = (int)(bulletPath.Length() / bulletSpeed[i]);//speed must be in units per second            

                if (timeToImpact[i] > hook.Hook.BulletLife)
                    timeToImpact[i] = int.MaxValue;
            });

            return (aimSpot, timeToImpact);
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
        public static float[] ProjectClosestIntersectionDist(HookComputer hook, API.Client.Body[] bullets, Vector2 start, Vector2 destination, float maxTime, Robot r)
        {
            int n = bullets.Length;
            
            float[] closeDist= new float[n];
            Campy.Parallel.For(n, i => { 
                var bullet=bullets[i];
            var path = destination - start;
            var pLen = path.Length();
            var bS = bullet.Position;
            var bM = bullet.Momentum;
            var targetPosition = start;
            var fromPosition = bS;
            var toTarget = targetPosition - fromPosition;

            var bulletSpeed = bullet.Momentum.Length();
            var targetMomentum = (destination - start) / ((float)maxTime);

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
            t = MathF.Max(MathF.Min(maxTime + 10.0f, t), 0.0f);

            var aimSpot = targetPosition + targetMomentum * t;
            var aimMinusS = aimSpot - start;
            var desMinusS = destination - start;
            var willHit = false;
            var disss = float.MaxValue;
            var bulletPath = aimSpot - fromPosition;
            var timeToImpact = (int)(bulletPath.Length() / bulletSpeed);//speed must be in units per second            

            if (timeToImpact > hook.Hook.BulletLife || (timeToImpact > maxTime + 10))
            {
                timeToImpact = int.MaxValue;
            }
            else
            {
                var themS = start + targetMomentum * ((float)timeToImpact);
                disss = (themS - aimSpot).Length();
            }
            closeDist[i]=disss;
            });
            return closeDist;

        }
    }
}
