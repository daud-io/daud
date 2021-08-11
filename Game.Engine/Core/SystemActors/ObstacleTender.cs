namespace Game.Engine.Core.SystemActors
{
    using Game.Engine.Core.Pickups;
    using System.Collections.Generic;

    public class ObstacleTender : SystemActorBase
    {
        private readonly List<IActor> Flock = new List<IActor>();
        public ObstacleTender(World world): base(world)
        {
            Flock.Add(new GenericTender<Obstacle>(world, () => World.Hook.Obstacles));
            Flock.Add(new GenericTender<Fish>(world, () => World.Hook.Fishes));
            Flock.Add(new GenericTender<PickupSeeker>(world, () => World.Hook.PickupSeekers));
            Flock.Add(new GenericTender<PickupShield>(world, () => World.Hook.PickupShields));
            Flock.Add(new GenericTender<PickupRobotGun>(world, () => World.Hook.PickupRobotGuns));
            Flock.Add(new GenericTender<HasteToken>(world, () => World.Hook.Tokens));

        }

        public override void Destroy()
        {
            foreach (var element in Flock)
                element.Destroy();

            base.Destroy();
        }
    }
}