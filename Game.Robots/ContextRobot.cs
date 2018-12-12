namespace Game.Robots
{
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class ContextRobot : Robot
    {
        protected override Task AliveAsync()
        {
            float angle = (float)((GameTime - SpawnTime) / 1000.0f) * MathF.PI * 2;
            var centerVector = -1 * this.Position;
            angle = MathF.Atan2(centerVector.Y, centerVector.X);

            var bullets = this.Bodies
                .Where(b => b.Sprite.ToString().StartsWith("bullet") || b.Sprite == API.Common.Sprites.seeker)
                .Where(b => b.Group?.Owner != FleetID)
                .OrderBy(b => Vector2.Distance(b.Position, Position))
                .ToList();

            if (bullets.Any())
            {
                var bullet = bullets.First();
                var distance = Vector2.Distance(bullet.Position, Position);
                if (distance < 2000)
                {
                    Console.WriteLine($"bullet.group.owner: {bullet.Group.Owner} myFleetID: {FleetID}");
                    var runAway = Position - bullet.Position;
                    angle = MathF.Atan2(runAway.Y, runAway.X);
                }

            }

            // if you're not actually doing any async/await, just return this
            return Task.FromResult(0);
        }

        protected override Task DeadAsync()
        {
            return base.DeadAsync();
        }
    }
}
