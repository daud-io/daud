namespace Game.Robots.Monsters
{
    using Game.Robots.Herding;
    using System;
    using System.Threading.Tasks;

    public class SpawnBeast : ConfigMonster
    {
        public SpawnBeast(Shepherd tender) : base(tender)
        { }


        protected async override Task OnDeathAsync()
        {
            for (int i=0; i<5; i++)
                await this.SpawnChildAsync<SpawnChild>("child.json");

            await base.OnDeathAsync();
        }
    }
}
