namespace Game.Engine.Core.SystemActors
{
    using System;
    using System.Collections.Generic;

    public class WorldResizer : SystemActorBase
    {
        private List<ArenaWall> Walls;
        
        public WorldResizer(World world): base(world)
        {
            CycleMS = 0;

            Walls = new List<ArenaWall>();
            Walls.Add(new ArenaWall(world, ArenaWall.WhichWall.North));
            Walls.Add(new ArenaWall(world, ArenaWall.WhichWall.East));
            Walls.Add(new ArenaWall(world, ArenaWall.WhichWall.South));
            Walls.Add(new ArenaWall(world, ArenaWall.WhichWall.West));
            
        }

        protected override void CycleThink()
        {
            var hook = World.Hook;
            if (World.Hook.WorldResizeEnabled)
            {
                int resizeCount = (World.AdvertisedPlayerCount < hook.WorldMinPlayersToResize)
                    ? 0
                    : World.AdvertisedPlayerCount - hook.WorldMinPlayersToResize + 1;

                int newSize = hook.WorldSizeBasic + resizeCount * hook.WorldSizeDeltaPerPlayer;
                if (hook.WorldSize < newSize)
                    hook.WorldSize = hook.WorldSize + hook.WorldResizeSpeed;
                else if (hook.WorldSize > newSize && hook.WorldSize - newSize > hook.WorldResizeSpeed)
                    hook.WorldSize = hook.WorldSize - hook.WorldResizeSpeed;

                hook.Obstacles = Convert.ToInt32(Math.Floor(hook.WorldSize * hook.ObstaclesMultiplier));
                hook.Fishes = Convert.ToInt32(Math.Floor(hook.WorldSize * hook.FishesMultiplier));
                hook.PickupSeekers = Convert.ToInt32(Math.Floor(hook.WorldSize * hook.PickupSeekersMultiplier));
                hook.PickupShields = Convert.ToInt32(Math.Floor(hook.WorldSize * hook.PickupShieldsMultiplier));
            }
        }
    }
}