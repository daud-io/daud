namespace Game.Robots.Monsters
{
    using Game.API.Common;
    using Game.Robots.Herding;

    public class SpawnChild : ConfigMonster
    {
        public SpawnChild(Shepherd tender) : base(tender)
        {
            this.Sprite = Sprites.ship_gray.ToString();

        }
    }
}
