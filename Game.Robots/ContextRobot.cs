namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Behaviors.Blending;
    using Game.Robots.Senses;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ContextRobot : Robot
    {
        protected readonly List<ISense> Sensors = new List<ISense>();
        protected readonly List<IBehaviors> Behaviors = new List<IBehaviors>();
        public readonly SensorBullets SensorBullets;
        public readonly SensorFleets SensorFleets;
        public readonly SensorTeam SensorTeam;

        protected IContextRingBlending ContextRingBlending { get; set; }

        public int Steps { get; protected set; }

        public ContextRobot()
        {
            Steps = 8;
            Sensors.Add(SensorBullets = new SensorBullets(this));
            Sensors.Add(SensorFleets = new SensorFleets(this));
            Sensors.Add(SensorTeam = new SensorTeam(this));

            ContextRingBlending = new ContextRingBlendingWeighted(this);
        }

        private void Sense()
        {
            foreach (var sensor in Sensors)
                sensor.Sense();
        }

        private void Behave()
        {

            var contexts = Behaviors.Select(b => b.Behave(Steps)).ToList();
            var angle = ContextRingBlending.Blend(contexts);

            SteerAngle(angle);
        }

        protected virtual Task OnSensors()
        {
            return Task.FromResult(0);
        }

        protected void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{this.FleetID}\t{this.Name}]\t");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        protected override Task OnDeathAsync()
        {
            this.Log("Oh snap, I'm dead.");
            return base.OnDeathAsync();
        }

        protected override Task OnSpawnAsync()
        {
            this.Log("Hooray, I'm alive!");
            return base.OnSpawnAsync();
        }

        protected async override Task AliveAsync()
        {
            Sense();

            await this.OnSensors();

            Behave();
        }
    }
}
