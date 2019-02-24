namespace Game.Engine.Core.SystemActors
{
    using System.Linq;

    public class Advertisement : SystemActorBase
    {
        protected override void CycleThink()
        {
            World.AdvertisedPlayerCount = Player.GetWorldPlayers(World)
                .Where(p => p.IsAlive || p.IsStillPlaying)
                .Where(p => !(p is Robot))
                .Count();
        }
    }
}