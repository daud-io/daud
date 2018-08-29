namespace Game.Engine.Core.Actors.Bots
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class Robot : Player
    {
        public bool AutoSpawn { get; set; } = true;
        public Robot() : base()
        {
        }

        public override void Step()
        {
            if (!IsAlive)
            {
                if (AutoSpawn)
                    this.Spawn();
                else
                    return;
            }

            this.Fleet.Color = "green";

            foreach (var player in
                GetWorldPlayers(World).OrderByDescending(p => p.Score)
                    .Where(p => !p.Fleet?.Caption?.StartsWith("Daud") ?? true)
                    .Where(p => p.IsAlive)
                )
            {
                var delta = Vector2.Subtract(player.Fleet.Position, this.Fleet.Position);

                var trueAngle = (float)Math.Atan2(delta.Y, delta.X); ;
                var quantized = (int)(trueAngle * 100) / 100f;
                this.ControlInput.Angle = quantized;
                this.ControlInput.ShootRequested = true;

                this.SetControl(ControlInput);
                break;
            }
            

            base.Step();
        }
    }
}