namespace Game.Engine.Core.Actors.Bots
{
    using System;
    using System.Numerics;

    public class Robot : Player
    {
        public Robot() : base()
        {
            ShootCooldownTime = 800;
            MaxHealth = 50;
            BaseThrust = 2;
        }

        public override void Step()
        {
            if (Deinitialized) return;

            if (!IsAlive)
                this.Spawn();

            base.ShootRequested = true;

            foreach (var obj in world.Objects)
            {
                if (obj != this.GameObject
                    && obj.ObjectType == "player"
                    && !(obj.Caption?.StartsWith("Daud") ?? false)
                )
                {
                    var delta = Vector2.Subtract(obj.Position, this.GameObject.Position);
                    Angle = (float)Math.Atan2(delta.Y, delta.X);
                }
            }

            base.Step();
        }

        public override void PostStep()
        {
        }
    }
}
