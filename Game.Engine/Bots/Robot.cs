namespace Game.Engine.Bots
{
    using Game.Engine.Core;
    using System;
    using System.Numerics;

    public class Robot : Player
    {
        public override void Step(World world)
        {
            foreach (var obj in world.Objects)
            {
                if (obj != this.GameObject)
                {
                    var delta = Vector2.Subtract(obj.Position, this.GameObject.Position);
                    Angle = (float)Math.Atan2(delta.Y, delta.X);
                }
            }

            base.Step(world);
        }

        public override void SetupView(World world)
        {
            
        }
    }
}
