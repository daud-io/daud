namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Behaviors.Blending;
    using Game.Robots.Senses;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class ContextRobot : Robot
    {
        protected readonly List<ISense> Sensors = new List<ISense>();
        protected List<ContextBehavior> Behaviors = new List<ContextBehavior>();
        public readonly SensorBullets SensorBullets;
        public readonly SensorFleets SensorFleets;
        public readonly SensorTeam SensorTeam;
        public readonly SensorFish SensorFish;
        public readonly SensorAbandoned SensorAbandoned;
        private ContextRing BlendedRing = null;

        protected IContextRingBlending ContextRingBlending { get; set; }

        public int Steps { get; set; }

        public bool RingDebugEnabled { get; set; } = false;

        public ContextRobot()
        {
            Steps = 8;
            Sensors.Add(SensorBullets = new SensorBullets(this));
            Sensors.Add(SensorFleets = new SensorFleets(this));
            Sensors.Add(SensorTeam = new SensorTeam(this));
            Sensors.Add(SensorFish = new SensorFish(this));
            Sensors.Add(SensorAbandoned = new SensorAbandoned(this));

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
            (var finalRing, var angle, var boost) = ContextRingBlending.Blend(contexts, false);
            BlendedRing = finalRing;
            SteerAngle(angle);
        }

        [Obsolete]
        protected virtual void OnFinalRing(ContextRing ring)
        {

        }

        public void SetBehaviors(IEnumerable<BehaviorDescriptor> behaviors)
        {
            this.Behaviors = behaviors.Select(descriptor =>
            {
                var type = Type.GetType(descriptor.BehaviorTypeName);
                var behavior = Activator.CreateInstance(type, this) as ContextBehavior;

                behavior.BehaviorWeight = descriptor.BehaviorWeight;
                behavior.Cycle = descriptor.Cycle;
                behavior.Plot = descriptor.Plot;
                behavior.LookAheadMS = descriptor.LookAheadMS;

                if (descriptor.Config != null)
                {
                    // there should probably be a way to do this without json.. but this is neat.
                    var json = JsonConvert.SerializeObject(descriptor.Config);
                    JsonConvert.PopulateObject(json, behavior);
                }

                return behavior;
            }).ToList();
        }


        protected virtual Task OnSensors()
        {
            return Task.FromResult(0);
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
            RingDebugExecute();
        }


        class PlotTrace
        {
            public IEnumerable<float> r { get; set; }
            public string name { get; set; }
            public float opacity { get; set; } = 0.5f;
            public string type { get; set; } = "barpolar";
        }

        protected void RingDebugExecute()
        {
            if (!RingDebugEnabled)
                return;


            var traces = new List<PlotTrace>();

            traces.AddRange(this.Behaviors.Where(b => b.Plot).Select(b => new PlotTrace
            {
                r = b.LastRing?.Weights.Select(w => w * b.BehaviorWeight),
                name = b.LastRing?.Name
            }));

            if (this.BlendedRing != null)
                traces.Add(new PlotTrace
                {
                    r = this.BlendedRing.Weights,
                    name = "blended"
                });

            this.CustomData = JsonConvert.SerializeObject(new
            {
                plotly = new
                {
                    data = traces,
                    layout = new
                    {
                        paper_bgcolor = "rgba(0,0,0,0)",
                        plot_bgcolor = "rgba(0,0,0,0)",
                        hovermode = false,
                        polar = new
                        {
                            bgcolor = "rgba(0,0,0,0)",
                            angularaxis = new
                            {
                                rotation = 0,
                                direction = "clockwise"
                            }
                        }
                    }
                }
            });
        }

    }
}
