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
            Parallel.For(0, N, i =>
            {
                var bS = bullet[i].Position;
                var bM = bullet[i].Momentum;

                var fromPosition = bS;

                var bulletSpeed = bullet[i].Momentum.Length();
                var targetMomentum = (destination - start) / ((float)maxTime);

                var ab = targetPosition - fromPosition;
                var dist = ab.Length();
                var ui = targetMomentum - Vector2.Dot(targetMomentum, ab / dist) * ab / dist;
                var disss = float.MaxValue;
                var vj_mag2 = bulletSpeed * bulletSpeed - ui.LengthSquared();
                if (vj_mag2<0.0f){fout[i]=disss;return;}
                var aimSpot = ab + ui * dist / MathF.Sqrt(vj_mag2);
                var timeToImpact = (int)(aimSpot.Length() / bulletSpeed);

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
