namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Behaviors.Blending;
    using Game.Robots.Models;
    using Game.Robots.Senses;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class DogeBot : ContextRobot
    {
        protected readonly NavigateToPoint Navigation;
        protected readonly Efficiency Efficiency;
        // protected readonly DodgeNew DodgeF;
        // protected readonly DodgeNew Dodge0;
        // protected readonly DodgeNew Dodge1;
        // protected readonly DodgeNew DodgeNewd;
        protected readonly StayInBounds StayInBounds;
        protected readonly SeparationClose Separation;

        public readonly SensorCTF SensorCTF;

        public Vector2 ViewportCrop { get; set; } = new Vector2(2000 * 16f / 9f, 2000);
        public int BoostThreshold { get; set; } = 1;
        public float BoostDangerThreshold { get; set; } = 4;


        public DogeBot()
        {
            ContextRingBlending = new ContextRingBlendingWeighted(this);
            Behaviors.Add(Navigation = new NavigateToPoint(this) { BehaviorWeight = 0.001f });
            Behaviors.Add(Efficiency = new Efficiency(this) { BehaviorWeight = 0.0f });
            // Behaviors.Add(DodgeF = new DodgeNew(this) { LookAheadMS = 50, BehaviorWeight = 2 });
            // Behaviors.Add(Dodge0 = new DodgeNew(this) { LookAheadMS = 100, BehaviorWeight = 2 });
            // Behaviors.Add(Dodge1 = new DodgeNew(this) { LookAheadMS = 150, BehaviorWeight = 2 });
            // Behaviors.Add(DodgeNewd = new DodgeNew(this) { LookAheadMS = 200, BehaviorWeight = 2 });
            for(var i=100;i<2000;i+=100){
                Behaviors.Add( new DogeWow(this) { LookAheadMS = i, BehaviorWeight = 2/((float)(i))*100.0f });
            }
            Behaviors.Add(Separation = new SeparationClose(this) { LookAheadMS = 500, BehaviorWeight = 5.0f });
            Behaviors.Add(StayInBounds = new StayInBounds(this) { LookAheadMS = 1000, BehaviorWeight = 1f });

            Sensors.Add(SensorCTF = new SensorCTF(this));

            //Navigation.TargetPoint = target;
            Steps = 16;
        }

        protected async override Task AliveAsync()
        {
            foreach (var sensor in Sensors)
                sensor.Sense();
            if (CanShoot)
            {
                var closest = SensorFleets.Others
                    .Select(f => new { Fleet = f, Distance = Vector2.Distance(this.Position, f.Center) })
                    .Where(p => MathF.Abs(p.Fleet.Center.X - this.Position.X) <= ViewportCrop.X
                        && MathF.Abs(p.Fleet.Center.Y - this.Position.Y) <= ViewportCrop.Y)
                    .Where(p => !HookComputer.TeamMode || p.Fleet.Color != this.Color)
                    .OrderBy(p => p.Distance)
                    .FirstOrDefault()
                    ?.Fleet;

                if (closest != null){
                    ShootAtFleet(closest);
                }else{
                    if(SensorFleets.MyFleet!=null){
                       ShootAt(SensorFleets.MyFleet.Momentum);//*100.0f+this.Position);
                    }
                }
            }

            
            Navigation.TargetPoint=new Vector2(this.Position.Y,-this.Position.X);
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
            var bangle=0.0f;
            if(SensorFleets.MyFleet!=null){
                bangle=MathF.Atan2(this.SensorFleets.MyFleet.Momentum.Y,this.SensorFleets.MyFleet.Momentum.X);
            }
            var angle = ContextRingBlending.Blend(contexts);
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

                
            //(maxIndex+this.Steps/2)%this.Steps
            if (CanBoost && (SensorFleets.MyFleet?.Ships.Count ?? 0) > BoostThreshold && combined.Weights[minIndex]-combined.Weights[maxIndex]<-BoostDangerThreshold)
                Boost();
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
