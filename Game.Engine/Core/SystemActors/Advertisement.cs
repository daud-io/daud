namespace Game.Engine.Core.SystemActors
{
    using System;
    using System.Linq;

    public class Advertisement : SystemActorBase
    {
        private uint EmptySince = 0;

        protected override void CycleThink()
        {
            World.AdvertisedPlayerCount = Player.GetWorldPlayers(World)
                .Where(p => p.IsAlive || p.IsStillPlaying)
                .Where(p => !(p is Robot))
                .Count();

            if (World.AdvertisedPlayerCount > 0)
                EmptySince = World.Time;


            if (World.Hook.AutoRemoveOnEmptyThreshold > 0 && EmptySince > 0 && (World.Time - EmptySince) > World.Hook.AutoRemoveOnEmptyThreshold)
            {
                Worlds.Destroy(World.WorldKey);
            }
        }
    }
}