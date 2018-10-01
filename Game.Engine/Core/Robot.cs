namespace Game.Engine.Core
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

        protected override Fleet CreateFleet(string name, string color)
        {
            return new RobotFleet
            {
                Owner = this,
                Position = World.RandomPosition(),
                Caption = name,
                Color = color
            };

        }

        public override void Step()
        {
            if (!IsAlive)
            {
                if (AutoSpawn)
                    this.Spawn(Name, ShipSprite, "green");
                else
                    return;
            }


            var player =
                GetWorldPlayers(World).OrderByDescending(p => p.Score)
                    .Where(p => !p.Fleet?.Caption?.StartsWith("Daud") ?? true)
                    .Where(p => p.IsAlive)
                    .Where(p => (p.Fleet?.Ships?.Count() ?? 0) > 0)
                    .OrderBy(p => Vector2.Distance(p.Fleet.Position, this.Fleet.Position))
                    .FirstOrDefault();

            if (player != null)
            { 
                var delta = Vector2.Subtract(player.Fleet.Position, this.Fleet.Position);

                var trueAngle = (float)Math.Atan2(delta.Y, delta.X);
                var quantized = (int)(trueAngle * 100) / 100f;
                this.ControlInput.Angle = quantized;

                if (float.IsNaN(delta.X))
                {
                }
                this.ControlInput.Position = delta;

                this.ControlInput.ShootRequested = true;

                this.SetControl(ControlInput);
            }
            else
            {
                this.ControlInput.Angle = (float)Math.Atan2(-Fleet.Position.Y, -Fleet.Position.X);
                this.ControlInput.Position = new Vector2(MathF.Cos(this.ControlInput.Angle), MathF.Sin(this.ControlInput.Angle));
                if (float.IsNaN(this.ControlInput.Position.X))
                {
                }

                this.SetControl(ControlInput);
            }

            base.Step();
        }
    }
}