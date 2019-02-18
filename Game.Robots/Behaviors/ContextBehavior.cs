namespace Game.Robots.Behaviors
{
    using System.Numerics;
    using System;
    public class ContextBehavior : IBehavior
    {
        public virtual float BehaviorWeight { get; set; } = 1f;
        protected readonly ContextRobot Robot;
        public int LookAheadMS { get; set; } = 100;
        private long SleepUntil = 0;
        public ContextRing LastRing = null;
        public int Cycle = 0;
        public bool Plot { get; set; }
        public bool Normalize { get; set; } = true;

        public ContextBehavior(ContextRobot robot)
        {
            this.Robot = robot;
        }

        public ContextRing Behave(int steps)
        {
            if (this.Robot.GameTime > SleepUntil)
            {
                //Console.WriteLine("Processing");
                var ring = new ContextRing(steps);
                this.PreSweep(ring);

                if (Robot?.SensorFleets?.MyFleet?.Ships != null)
                {
                    for (var i = 0; i < steps; i++)
                    {
                        var momentum = Robot.SensorFleets.MyFleet.Momentum;
                        var position = RoboMath.ShipThrustProjection(Robot.HookComputer,
                            Robot.Position,
                            ref momentum,
                            Robot.SensorFleets.MyFleet.Ships.Count,
                            ring.Angle(i),
                            LookAheadMS
                        );
                        var momentumBoost = momentum/momentum.Length()*Robot.HookComputer.Hook.BoostThrust;
                        var positionBoost = RoboMath.ShipThrustProjection(Robot.HookComputer,
                            position,
                            ref momentumBoost,
                            Robot.SensorFleets.MyFleet.Ships.Count,
                            ring.Angle(i),
                            Math.Min(Robot.HookComputer.Hook.BoostDuration,LookAheadMS)
                        );

                        ring.Weights[i] = ScoreAngle(ring.Angle(i), position, momentum);
                        ring.WeightsBoost[i] = ScoreAngle(ring.Angle(i), positionBoost, momentumBoost);
                    }
                }

                ring.RingWeight = BehaviorWeight;

                this.PostSweep(ring);

                ring.Name = this.GetType().Name;
                LastRing = ring;

                if (Cycle > 0)
                    Sleep(Cycle);

                return ring;
            }
            else
            {
                //Console.WriteLine("Waiting");
                return new ContextRing(LastRing);
            }
        }

        protected void Sleep(int ms)
        {
            SleepUntil = this.Robot.GameTime + ms;
        }

        public virtual void Reset()
        {
        }

        protected virtual void PreSweep(ContextRing ring)
        {
        }

        protected virtual void PostSweep(ContextRing ring)
        {
            if (Normalize)
                ring.Normalize();
        }

        protected virtual float ScoreAngle(float angle, Vector2 position, Vector2 momentum) => 0f;
    }
}
