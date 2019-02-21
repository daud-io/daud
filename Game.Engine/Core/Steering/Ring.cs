namespace Game.Engine.Core.Steering
{
    using System;
    using System.Linq;
    using System.Numerics;

    public static class Ringing
    {
        private const int RingThreshold = 5;

        public static void Ring(Ship ship)
        {
            var hook = ship.World.Hook;
            var fleet = ship.Fleet;

            if (fleet == null || !fleet.BossMode)
                return;

            if (fleet.Ships.Count <= RingThreshold)
                return;

            var center = FleetMath.FleetCenterNaive(fleet.Ships.Take(RingThreshold));
            var momentum = FleetMath.FleetMomentum(fleet.Ships.Take(RingThreshold));

            var targetAngle = MathF.Atan2(fleet.AimTarget.Y, fleet.AimTarget.X);
            var shipIndex = fleet.Ships.IndexOf(ship);
            var innerAngle = (shipIndex - RingThreshold) / (float)(fleet.Ships.Count - RingThreshold) * 2 * MathF.PI;
            var angle = (shipIndex - RingThreshold) / (float)(fleet.Ships.Count - RingThreshold) * 2 * MathF.PI;
            if (shipIndex > (RingThreshold-1))
            {
                ship.Position = center / RingThreshold +
                    new Vector2(
                        MathF.Cos(angle + targetAngle),
                        MathF.Sin(angle + targetAngle)
                    ) * (50 + 15 * fleet.Ships.Count);
                ship.Momentum = momentum / RingThreshold;
                ship.Angle = angle + targetAngle;
            }
        }
    }
}
