namespace Game.Robots
{
    using Game.Robots.Behaviors;
    using Game.Robots.Models;
    using Game.Robots.Senses;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class MadBot : ContextRobot
    {
        protected readonly NavigateToPoint Navigation;
        protected readonly Efficiency Efficiency;
        protected readonly Dodge Dodge0;
        protected readonly Dodge Dodge1;
        protected readonly Dodge Dodge2;
        protected readonly StayInBounds StayInBounds;
        protected readonly Separation Separation;

        public readonly SensorCTF SensorCTF;

        public Vector2 ViewportCrop { get; set; } = new Vector2(2000 * 16f / 9f, 2000);
        public int BoostThreshold { get; set; } = 16;


        public MadBot()
        {
            Behaviors.Add(Navigation = new NavigateToPoint(this) { BehaviorWeight = 0.01f });
            Behaviors.Add(Efficiency = new Efficiency(this) { BehaviorWeight = 0.1f });
            Behaviors.Add(Dodge0 = new Dodge(this) { LookAheadMS = 250, BehaviorWeight = 2 });
            Behaviors.Add(Dodge1 = new Dodge(this) { LookAheadMS = 500, BehaviorWeight = 2 });
            Behaviors.Add(Dodge2 = new Dodge(this) { LookAheadMS = 1000, BehaviorWeight = 2 });
            Behaviors.Add(Separation = new Separation(this) { LookAheadMS = 500, BehaviorWeight = 0f });
            Behaviors.Add(StayInBounds = new StayInBounds(this) { LookAheadMS = 1000, BehaviorWeight = 1f });

            Sensors.Add(SensorCTF = new SensorCTF(this));

            Navigation.TargetPoint = new Vector2(0, 0);
            Steps = 16;
        }

        protected async override Task AliveAsync()
        {
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
                        ShootAt(SensorFleets.MyFleet.Momentum);
                    }
                }
            }

            if (CanBoost && (SensorFleets.MyFleet?.Ships.Count ?? 0) > BoostThreshold&& false)
                Boost();


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

            await base.AliveAsync();
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
