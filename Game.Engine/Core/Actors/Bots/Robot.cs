namespace Game.Engine.Core.Actors.Bots
{
    using System;
    using System.Linq;
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

            foreach (var player in
                world.Players.OrderByDescending(p => p.Score)
                    .Where(p => !p.Name?.StartsWith("Daud") ?? false)
                    )
            {
                var delta = Vector2.Subtract(player.GameObject.Position, this.GameObject.Position);
                Angle = (float)Math.Atan2(delta.Y, delta.X);
                break;
            }

            base.Step();
        }

        public override void PostStep()
        {
        }
    }
}
