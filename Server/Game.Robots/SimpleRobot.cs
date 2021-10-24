namespace Game.Robots
{
    using Game.Robots.Senses;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class SimpleRobot : ContextRobot
    {
        protected override Task AliveAsync()
        {

            SteerPointAbsolute(Vector2.Zero); // center of the universe

            var vel = Vector2.Zero;
            var fleets = false;
            if (SensorFleets.AllVisibleFleets.Any())
            {
                var fleet = SensorFleets.AllVisibleFleets.FirstOrDefault(f => f.ID != FleetID);
                if (fleet != null)
                {
                    fleets = true;
                    vel += (fleet.Center - Position) * 1;
                }
            }
            if (!fleets)
            {
                var angle = (float)((GameTime - SpawnTime) / 3000.0f) * MathF.PI * 2;
                vel.X += (float)Math.Cos(angle);
                vel.Y += (float)Math.Sin(angle);
            }

            var bullets = SensorBullets.VisibleBullets;
            var danger = false;

            if (bullets.Any())
            {
                var bullet = bullets.First();
                var distance = Vector2.Distance(bullet.Position, Position);
                if (distance < 2000)
                {
                    var avoid = (Position - bullet.Position);
                    vel += avoid * 400_000 / avoid.LengthSquared();
                }
                if (distance < 200)
                {
                    danger = true;
                }
            }

            SetSplit(danger);

            SteerAngle(MathF.Atan2(vel.Y, vel.X));

            // if you're not actually doing any async/await, just return this
            return Task.FromResult(0);
        }
    }
}
