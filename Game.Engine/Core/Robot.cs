namespace Game.Engine.Core
{
    using System;
    using System.Linq;
    using System.Numerics;

    public class Robot : Player
    {
        public bool AutoSpawn { get; set; } = true;

        private long SpawnTimeAfter = 0;

        public Robot() : base()
        {
        }

        protected override Fleet CreateFleet(string color)
        {
            return new RobotFleet
            {
                Owner = this,
                Caption = this.Name
            };
        }


        public override void CreateDestroy()
        {
            base.CreateDestroy();

            if (!IsAlive)
            {
                if (AutoSpawn && World.Time > SpawnTimeAfter)
                    this.Spawn(Name, ShipSprite, "green", "");
            }
        }

        protected override void OnDeath(Player player = null)
        {
            base.OnDeath(player);

            SpawnTimeAfter = World.Time + World.Hook.BotRespawnDelay;
        }

        public override void Think()
        {
            if (!IsAlive)
                return;

            var player =
                GetWorldPlayers(World).OrderByDescending(p => p.Score)
                    .Where(p => p.IsAlive)
                    .Where(p => (p.Fleet?.Ships?.Count() ?? 0) > 0)
                    .Where(p => !p.Name?.StartsWith("Daud") ?? true)
                    .OrderBy(p => Vector2.Distance(p.Fleet.FleetCenter, this.Fleet.FleetCenter))
                    .FirstOrDefault();

            if (player != null)
            {
                var delta = Vector2.Subtract(player.Fleet.FleetCenter, this.Fleet.FleetCenter);

                var trueAngle = (float)Math.Atan2(delta.Y, delta.X);
                var quantized = (int)(trueAngle * 100) / 100f;

                this.ControlInput.Position = delta;

                this.ControlInput.ShootRequested = true;

                this.SetControl(ControlInput);
            }
            else
            {
                var angle = (float)Math.Atan2(-Fleet.FleetCenter.Y, -Fleet.FleetCenter.X);
                this.ControlInput.Position = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                this.SetControl(ControlInput);
            }

            base.Think();
        }
    }
}