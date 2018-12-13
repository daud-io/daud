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

            SteerPointAbsolute(Vector2.Zero); // center of the universe

            if (SensorFleets.VisibleFleets.Any())
            {
                var fleet = SensorFleets.VisibleFleets.FirstOrDefault(f => f.ID != FleetID);
                if (fleet != null)
                    SteerPointAbsolute(fleet.Center);
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
                    SteerAngle(MathF.Atan2(runAway.Y, runAway.X));
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
