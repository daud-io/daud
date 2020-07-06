namespace Game.Robots.Monsters
{
    using Game.API.Common;
    using Game.API.Common.Models;
    using Game.Robots.Herding;
    using System.Numerics;
    using System.Threading.Tasks;

    public class Territory : ConfigMonster
    {

        public Territory(Shepherd tender) : base(tender)
        {
        }

        protected override Task OnSpawnAsync()
        {
            return base.OnSpawnAsync();
        }

        protected override Task OnKilledAsync(Announcement announcement)
        {
            SwapTeams();
            Color = "monster";
            return base.OnKilledAsync(announcement);
        }

        protected async override Task AliveAsync()
        {
            if (!IsAlive)
                return;

            if (Color == "monster")
                Color = Sprite;

            if (Vector2.Distance(SensorFleets.LastKnownCenter, Vector2.Zero) > 200)
            {
                NextPosition = Vector2.Zero;
            }
            await base.AliveAsync();
        }

        private void SwapTeams()
        {
            if (Sprite == "ship_cyan")
            {
                Sprite = "ship_red";
            }
            else
            {
                Sprite = "ship_cyan";
            }
        }
    }
}
