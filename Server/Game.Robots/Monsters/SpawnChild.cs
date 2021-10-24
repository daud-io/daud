namespace Game.Robots.Monsters
{
    using Game.API.Common;
    using Game.Robots.Herding;
    using System.Threading.Tasks;

    public class SpawnChild : ConfigMonster
    {
        public SpawnChild(Shepherd tender) : base(tender)
        {
            this.Sprite = Sprites.ship_gray.ToString();
        }

        protected override Task OnDeathAsync()
        {
            return base.OnDeathAsync();
        }
    }
}
