namespace Game.Engine.Bots
{
    using Game.Engine.Core;
    using System;
    using System.Numerics;

    public class Robot : Player
    {
        public Robot(World world) : base(world)
        {
            ShootCooldownTime = 800;
            MaxHealth = 50;
            BaseThrust = 2;
        }
        public override void Step()
        {
            base.ShootRequested = true;

            foreach (var obj in world.Objects)
            {
                if (obj != this.GameObject && obj.ObjectType == "player")
                {
                    var delta = Vector2.Subtract(obj.Position, this.GameObject.Position);
                    Angle = (float)Math.Atan2(delta.Y, delta.X);
                }
            }


            base.Step();
        }

        public override void Hit(Bullet bullet)
        {
            base.Hit(bullet);
        }

        public override void SetupView()
        {
            
        }

        public override void Die()
        {
            base.Die();

            this.Spawn();
        }
    }
}
