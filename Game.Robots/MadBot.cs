namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Models;
    using Game.Robots.Senses;
    using Game.Robots.Targeting;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class MadBot : ContextRobot
    {
        protected readonly NavigateToPoint Navigation;
        protected readonly Efficiency Efficiency;
        // protected readonly Dodge Dodge0;
        // protected readonly Dodge Dodge1;
        // protected readonly Dodge Dodge2;
        private readonly FleetTargeting FleetTargeting;
        private readonly AbandonedTargeting AbandonedTargeting;
        private readonly FishTargeting FishTargeting;
        protected readonly StayInBounds StayInBounds;
        protected readonly Separation Separation;

        public readonly SensorCTF SensorCTF;

        public Vector2 ViewportCrop { get; set; } = new Vector2(2000 * 16f / 9f, 2000);
        public int BoostThreshold { get; set; } = 1;
        public float BoostDangerThreshold { get; set; } = -7.0f;
        public float MidBad = 0.5f;



        public MadBot()
        {
            FleetTargeting = new FleetTargeting(this);
            AbandonedTargeting = new AbandonedTargeting(this);
            FishTargeting = new FishTargeting(this);
            Behaviors.Add(Navigation = new NavigateToPoint(this) { BehaviorWeight = 1.0f });
            // Behaviors.Add(Efficiency = new Efficiency(this) { BehaviorWeight = 0.0f });
            // Behaviors.Add(Dodge0 = new Dodge(this) { LookAheadMS = 100, BehaviorWeight = 2 });
            // Behaviors.Add(Dodge1 = new Dodge(this) { LookAheadMS = 200, BehaviorWeight = 2 });
            // Behaviors.Add(Dodge2 = new Dodge(this) { LookAheadMS = 300, BehaviorWeight = 2 });
            for (int m = 000; m < 2000; m += 50)
            {
                Behaviors.Add(new DogeWow(this) { LookAheadMS = m + 50, BehaviorWeight = 1 });
            }
            Behaviors.Add(Separation = new Separation(this) { LookAheadMS = 500, BehaviorWeight = 2.0f });
            Behaviors.Add(StayInBounds = new StayInBounds(this) { LookAheadMS = 1000, BehaviorWeight = 10f });

            Sensors.Add(SensorCTF = new SensorCTF(this));

            Navigation.TargetPoint = new Vector2(0, 0);
            Steps = 16;
        }

        protected async override Task AliveAsync()
        {
            foreach (var sensor in Sensors)
                sensor.Sense();





            if (SensorCTF.CTFModeEnabled)
            {
                if (SensorCTF.IsCarryingFlag)
                {
                    // I'm carrying the flag

                    if (SensorCTF.OurTeam.FlagIsHome)
                        // our flag is home, head to base to score
                        Navigation.TargetPoint = SensorCTF.OurTeam.BasePosition;
                    else
                        // our flag is not home, attack the guy who stole it
                        // this seems required for 1v1, we might get trickier and
                        // change behavior here if we have a teammate
                        Navigation.TargetPoint = SensorCTF.OurTeam.FlagPosition;
                }
                else
                {
                    // I'm not carrying a flag

                    if (!SensorCTF.TheirTeam.FlagIsHome)
                    {
                        // our teammate is carrying a flag
                        if (!SensorCTF.OurTeam.FlagIsHome)
                            // our flag is not home, attack it
                            Navigation.TargetPoint = SensorCTF.OurTeam.FlagPosition;
                        else
                            // our flag is home, defend teammate
                            Navigation.TargetPoint = SensorCTF.TheirTeam.FlagPosition;
                    }
                    else
                    {
                        // their flag is home
                        Navigation.TargetPoint = SensorCTF.TheirTeam.BasePosition;
                    }
                }
            }

            var contexts = Behaviors.Select(b => b.Behave(Steps)).ToList();
            var bangle = 0.0f;
            if (SensorFleets.MyFleet != null)
            {
                bangle = MathF.Atan2(this.SensorFleets.MyFleet.Momentum.Y, this.SensorFleets.MyFleet.Momentum.X);
            }
            (var finalRing, var angle, var boost) = ContextRingBlending.Blend(contexts, false);
            OnFinalRing(finalRing);
            var combined = new ContextRing(this.Steps);


            // blur

            // lock (typeof(ContextRingBlendingWeighted))
            // {
            //     Console.SetCursorPosition(0, 0);
            //     Console.WriteLine("RingDump");
            //     foreach (var context in contexts)
            //     {
            //         var name = context.Name;
            //         while (name.Length < 20)
            //             name += ' ';
            //         Console.WriteLine($"{name}\t{string.Join(',', context.Weights.Select(w => (w * context.RingWeight).ToString("+0.0;-0.0")))}");
            //     }
            // }

            if (contexts.Any())
            {
                for (var i = 0; i < this.Steps; i++)
                    combined.Weights[i] = contexts.Sum(c => c.Weights[i] * c.RingWeight);

                var maxIndex = 0;
                var minIndex = 0;

                for (var i = 0; i < this.Steps; i++)
                {
                    if (combined.Weights[i] > combined.Weights[maxIndex])
                        maxIndex = i;
                    if (combined.Weights[i] < combined.Weights[minIndex])
                        minIndex = i;
                }



                if (CanBoost && (SensorFleets.MyFleet?.Ships.Count ?? 0) > BoostThreshold && (combined.Weights[maxIndex] < BoostDangerThreshold || (SensorFleets.MyFleet?.Ships.Count ?? 0) > 108))
                    Boost();

                this.MidBad = combined.Weights[maxIndex] / 2.0f + combined.Weights[minIndex] / 2.0f;

                if (CanShoot)
                {

                    var target = FleetTargeting.ChooseTarget()
                        ?? AbandonedTargeting.ChooseTarget()
                        ?? FishTargeting.ChooseTarget();

                    if (target != null)
                    {


                        var fff = target;
                        Vector2 sp = fff.Position - this.Position;
                        var angleg = (int)(MathF.Atan2(sp.Y, sp.X) / MathF.Atan2(0.0f, -1.0f) / 2.0f * Steps);
                        if (true)//combined.Weights[((angleg)%Steps+Steps)%Steps]>combined.Weights[minIndex])
                            ShootAt(sp + this.Position);
                    }
                    else
                    {

                        if (SensorFleets.MyFleet != null)
                        {
                            ShootAt(SensorFleets.MyFleet.Momentum);
                        }
                    }
                }
            }



            SteerAngle(angle);
        }
        protected override Task OnNewLeaderboardAsync()
        {

            return Task.FromResult(0);
        }

        private void ShootAtFleet(Fleet f)
        {
            ShootAt(
                RoboMath.FiringIntercept(
                    HookComputer,
                    this.Position,
                    f.Center,
                    f.Momentum,
                    this.SensorFleets.MyFleet?.Ships.Count ?? 0
                )
            );
        }
    }
}
