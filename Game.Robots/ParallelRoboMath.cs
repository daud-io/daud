namespace Game.Robots
{
    using Game.Robots.Models;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    public static class ParallelRoboMath
    {
        public static Vector2[] ProjectClosest(HookComputer hook, float maxTime, Vector2[] fromPosition, Vector2[] targetPosition, int[] fleetSize)
        {
            var boostSpeed = hook.Hook.BoostThrust;
            int N = fromPosition.Length;
            Vector2[] vout = new Vector2[N];
            for (int i = 0; i < N; i++)
            {
                var bulletSpeed = hook.ShotThrust(fleetSize[i]) * 10;
                var path = targetPosition[i] - fromPosition[i];
                var pLen = path.Length();
                var maxD = bulletSpeed * maxTime + boostSpeed * hook.Hook.BoostDuration;
                vout[i] = fromPosition[i] + path * (1.0f / pLen) * MathF.Min(pLen - 10.0f, maxD);
            }
            return vout;
        }
        public static float[] ProjectClosestIntersectionDist(HookComputer hook, float maxTime, Robot r, Vector2 start, Vector2 destination, API.Client.Body[] bullet)
        {
            var path = destination - start;
            var pLen = path.Length();
            var targetPosition = start;
            int N = bullet.Length;
            float[] fout = new float[N];
            Parallel.For(0,N,i=>{
                var bS = bullet[i].Position;
                var bM = bullet[i].Momentum;

                var fromPosition = bS;
                var toTarget = targetPosition - fromPosition;

                var bulletSpeed = bullet[i].Momentum.Length();
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
                fout[i] = disss;
            });
            return fout;
        }
    }
}
