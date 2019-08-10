namespace Game.Robots.Targeting
{
    using System;
    using System.Numerics;

    public abstract class TargetingBase
    {
        public Vector2 ViewportCrop { get; set; } = new Vector2(2000 * 16f / 9f, 2000);
        protected ContextRobot Robot;

        public Func<float, bool> IsSafeShot { get; set; } = (a) => true;

        public TargetingBase(ContextRobot robot)
        {
            this.Robot = robot;
        }

        public abstract Target ChooseTarget();

        protected bool IsInViewport(Vector2 point)
        {
            return MathF.Abs(point.X - this.Robot.Position.X) <= ViewportCrop.X
                && MathF.Abs(point.Y - this.Robot.Position.Y) <= ViewportCrop.Y;
        }

        public bool IsSafeTarget(Vector2 target)
        {
            var toTarget = target - Robot.Position;
            var angle = MathF.Atan2(toTarget.Y, toTarget.X);

            return IsSafeShot(angle);
        }
    }
}
