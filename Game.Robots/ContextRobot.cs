namespace Game.Robots
{
    using Game.Robots.Senses;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class ContextRobot : Robot
    {
        private List<ISense> Sensors = new List<ISense>();
        private readonly SensorBullets SensorBullets;
        private readonly SensorFleets SensorFleets;

        public ContextRobot()
        {
            Sensors.Add(SensorBullets = new SensorBullets(this));
            Sensors.Add(SensorFleets = new SensorFleets(this));
        }

        private void Sense()
        {
            foreach (var sensor in Sensors)
                sensor.Sense();
        }

        protected override Task AliveAsync()
        {
            Sense();

            var centerVector = -1 * this.Position;

            float angle = MathF.Atan2(centerVector.Y, centerVector.X);

            if (SensorFleets.VisibleFleets.Any())
            {
                var x = SensorFleets.VisibleFleets.FirstOrDefault().Center - this.Position;
                angle = MathF.Atan2(x.Y, x.X);
            }

            var bullets = SensorBullets.VisibleBullets;

            if (bullets.Any())
            {
                var bullet = bullets.First();
                var distance = Vector2.Distance(bullet.Position, Position);
                if (distance < 2000)
                {
                    //Console.WriteLine($"bullet.group.owner: {bullet.Group.Owner} myFleetID: {FleetID}");
                    var runAway = Position - bullet.Position;
                    angle = MathF.Atan2(runAway.Y, runAway.X);
                }
            }

            SteerAngle(angle);

            // if you're not actually doing any async/await, just return this
            return Task.FromResult(0);
        }

        protected override Task DeadAsync()
        {
            return base.DeadAsync();
        }
    }
}
