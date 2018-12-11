namespace Game.Robots
{
    using Game.API.Client;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    public class Robot
    {
        public string Name { get; set; } = "Robot";
        public string Sprite { get; set; } = "ship0";
        public string Color { get; set; } = "ship0";

        public bool AutoSpawn { get; set; } = true;
        private DateTime LastSpawn = DateTime.MinValue;
        private readonly Connection Connection;
        private const int RESPAWN_FALLOFF = 1000;
        private readonly DateTime Born = DateTime.Now;

        public bool AutoFire { get; set; } = false;

        public Robot(Connection connection)
        {
            this.Connection = connection;
            this.Connection.OnView = this.OnView;
        }

        public async Task Start()
        {
            await this.Connection.ListenAsync();
        }

        private async Task OnView()
        {
            if (!Connection.IsAlive)
                await StepDeadAsync();
            else
                await StepAliveAsync();
        }

        private async Task StepAliveAsync()
        {

            float angle = (float)(DateTime.Now.Subtract(Born).TotalMilliseconds / 1000.0f) * MathF.PI * 2;

            var centerVector = -1 * this.Connection.Position;

            angle = MathF.Atan2(centerVector.Y, centerVector.X);

            var bullets = this.Connection.Bodies
                .Where(b => b.Sprite.ToString().StartsWith("bullet") || b.Sprite == API.Common.Sprites.seeker)
                .Where(b => b.Group?.Owner != this.Connection?.FleetID)
                .OrderBy(b => Vector2.Distance(b.Position, this.Connection.Position))
                .ToList();

            if (bullets.Any())
            {
                var bullet = bullets.First();
                var distance = Vector2.Distance(bullet.Position, this.Connection.Position);
                if (distance < 2000)
                {
                    Console.WriteLine($"bullet.group.owner: {bullet.Group.Owner} myFleetID: {Connection.FleetID}");
                    var runAway = Connection.Position - bullet.Position;
                    angle = MathF.Atan2(runAway.Y, runAway.X);
                }

            }

            this.Connection.ControlAimTarget = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 100;
            this.Connection.ControlIsShooting = AutoFire;

            var groups = Connection.Bodies
                .Select(b => b.Group?.Caption)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct();

            var groupStrings = string.Join(", ", groups.ToArray());

            Console.WriteLine($"objects: {Connection.Bodies.Count()} groups: {groupStrings}");

            await this.Connection.SendControlInputAsync();
        }

        private async Task StepDeadAsync()
        {
            if (AutoSpawn)
            {
                if (DateTime.Now.Subtract(LastSpawn).TotalMilliseconds > RESPAWN_FALLOFF)
                {
                    await Connection.SpawnAsync(Name, Sprite, Color);
                    LastSpawn = DateTime.Now;
                }
            }
        }
    }
}
