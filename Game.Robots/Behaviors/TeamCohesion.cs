namespace Game.Robots.Behaviors
{
    using System.Linq;
    using System.Numerics;

    public class TeamCohesion : TeamBehaviorBase
    {
        public Vector2? TargetPoint = null;

        public int MaximumRange { get; set; } = int.MaxValue;
        public int MinimumRange { get; set; } = 0;

        public TeamCohesion(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {

            base.PreSweep(ring);

            TargetPoint = null;
            if (LocalTeammates.Any())
            {
                float count = 0;
                int n=0;
                Vector2 accumulator = Vector2.Zero;
                var BestTeammates=LocalTeammates.Select(f =>
                {
                    return new
                    {
                        Fleet = f,
                        Distance = Vector2.Distance(this.Robot.Position, f.Center),
                    };
                })
                .OrderBy(p => p.Distance).Select(f =>
                f.Fleet);
                foreach (var fleet in BestTeammates)
                {
                    if(n>4){
                        break;
                    }
                    var distance = Vector2.Distance(fleet.Center, this.Robot.Position);
                    if (distance <= MaximumRange && distance >= MinimumRange)
                    {
                        accumulator += (fleet.Center+(fleet.Momentum*(1.0f))*250.0f);
                        count+=1.0f;
                        n+=1;
                    }
                }
                if (accumulator != Vector2.Zero && count > 0)
                    TargetPoint = accumulator / ((float)count)+(this.Robot.SensorFleets.MyFleet==null?Vector2.Zero:this.Robot.SensorFleets.MyFleet.Momentum*150.0f);
            }
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            if (TargetPoint != null)
                return ScoreAngleByTargetPoint(TargetPoint.Value, angle, position, momentum);
            else
                return 0;
        }


        protected override void PostSweep(ContextRing ring)
        {
            ring.Normalize();
        }
    }
}
