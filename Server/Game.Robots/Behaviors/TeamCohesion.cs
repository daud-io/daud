namespace Game.Robots.Behaviors
{
    using System.Linq;
    using System.Numerics;

    public class TeamCohesion : TeamBehaviorBase
    {
        public Vector2? TargetPoint = null;

        public int MaximumRange { get; set; } = int.MaxValue;
        public int MinimumRange { get; set; } = 0;
        public int MaxFleets { get; set; } = 2;

        public TeamCohesion(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {
            base.PreSweep(ring);

            TargetPoint = null;
            if (LocalTeammates.Any() && this.Robot.SensorFleets.MyFleet != null)
            {
                var myFleet = this.Robot.SensorFleets.MyFleet;
                float count = 0;
                int n = 0;
                Vector2 accumulator = Vector2.Zero;
                var BestTeammates = LocalTeammates.Select(f =>
                  {
                      return new
                      {
                          Fleet = f,
                          Distance = (this.Robot.Position - f.Center).Length(),
                          FrontDistance = Vector2.Dot(this.Robot.Position - f.Center, myFleet.Velocity / myFleet.Velocity.Length()),
                          BackDistance = -Vector2.Dot(this.Robot.Position - f.Center, myFleet.Velocity / myFleet.Velocity.Length()),
                          LeftDistance = Vector2.Dot(this.Robot.Position - f.Center, new Vector2(myFleet.Velocity.Y, -myFleet.Velocity.X) / myFleet.Velocity.Length()),
                          RightDistance = -Vector2.Dot(this.Robot.Position - f.Center, new Vector2(myFleet.Velocity.Y, -myFleet.Velocity.X) / myFleet.Velocity.Length()),
                      };
                  }).Select(f =>
                  {
                      return new
                      {
                          Fleet = f.Fleet,
                          Distance = f.Distance,
                          FrontDistance = f.FrontDistance < 0 ? float.MaxValue : f.FrontDistance,
                          BackDistance = f.BackDistance < 0 ? float.MaxValue : f.BackDistance,
                          LeftDistance = f.LeftDistance < 0 ? float.MaxValue : f.LeftDistance,
                          RightDistance = f.RightDistance < 0 ? float.MaxValue : f.RightDistance,
                      };
                  });

                var BestLeft = BestTeammates.OrderBy(p => p.LeftDistance).Select(f =>
                  f.Fleet);
                var BestRight = BestTeammates.OrderBy(p => p.RightDistance).Select(f =>
                  f.Fleet);
                var BestFront = BestTeammates.OrderBy(p => p.FrontDistance).Select(f =>
                  f.Fleet);
                var BestBack = BestTeammates.OrderBy(p => p.BackDistance).Select(f =>
                  f.Fleet);
                var tms = BestBack.Take(MaxFleets).Concat(BestLeft.Take(MaxFleets)).Concat(BestRight.Take(MaxFleets)).Concat(BestFront.Take(MaxFleets));
                foreach (var fleet in tms)
                {
                    var distance = Vector2.Distance(fleet.Center, this.Robot.Position);
                    if (distance <= MaximumRange && distance >= MinimumRange)
                    {
                        accumulator += (fleet.Center + (fleet.Velocity * (1.0f)) * LookAheadMS);
                        count += 1.0f;
                        n += 1;
                    }
                }
                if (accumulator != Vector2.Zero && count > 0)
                    TargetPoint = accumulator / ((float)count) + (this.Robot.SensorFleets.MyFleet == null ? Vector2.Zero : this.Robot.SensorFleets.MyFleet.Velocity * LookAheadMS);
            }
        }

        protected override float ScoreAngle(float angle, Vector2 position, Vector2 momentum)
        {
            if (TargetPoint != null)
                return ScoreAngleByTargetPoint(TargetPoint.Value, angle, position, momentum);
            else
                return 0;
        }
    }
}
