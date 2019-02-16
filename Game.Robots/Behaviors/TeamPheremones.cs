namespace Game.Robots.Behaviors
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class TeamPheremones : TeamBehaviorBase
    {
        public Vector2? TargetPoint = null;

        public int MaximumRange { get; set; } = int.MaxValue;
        public int MinimumRange { get; set; } = 0;

        public TeamPheremones(ContextRobot robot) : base(robot)
        {
        }

        protected override void PreSweep(ContextRing ring)
        {

            base.PreSweep(ring);

            TargetPoint = null;
            if (RemoteTeamMates.Any())
            {
                int count = 0;
                Vector2 accumulator = Vector2.Zero;
                foreach (var remote in RemoteTeamMates)
                {
                    var distance = Vector2.Distance(remote, this.Robot.Position);
                    if (distance <= MaximumRange && distance >= MinimumRange)
                    {
                        accumulator += remote;
                        count++;
                    }
                }
                if (accumulator != Vector2.Zero && count > 0)
                    TargetPoint = accumulator / count;
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
