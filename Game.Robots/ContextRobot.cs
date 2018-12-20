namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Senses;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ContextRobot : Robot
    {
        protected readonly List<ISense> Sensors = new List<ISense>();
        protected readonly List<IBehaviors> Behaviors = new List<IBehaviors>();
        protected readonly SensorBullets SensorBullets;
        protected readonly SensorFleets SensorFleets;
        protected readonly int Steps;

        public ContextRobot()
        {
            Steps = 8;
            Sensors.Add(SensorBullets = new SensorBullets(this));
            Sensors.Add(SensorFleets = new SensorFleets(this));
        }

        private void Sense()
        {
            foreach (var sensor in Sensors)
                sensor.Sense();
        }

        private void Behave()
        {
            var contexts = Behaviors.Select(b => b.Behave(Steps)).ToList();
            var combined = new ContextRing(Steps);

            if (contexts.Any())
            {
                for (var i=0; i<Steps; i++)
                    combined.Weights[i] = contexts.Sum(c => c.Weights[i]);

                var maxIndex = 0;

                for (var i=0; i<Steps; i++)
                {
                    if (combined.Weights[i] > combined.Weights[maxIndex])
                        maxIndex = i;
                }

                SteerAngle(combined.Angle(maxIndex));
            }
        }

        protected virtual Task OnSensors()
        {
            return Task.FromResult(0);
        }

        protected async override Task AliveAsync()
        {
            Sense();

            await this.OnSensors();

            Behave();
        }
    }
}
