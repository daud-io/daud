namespace Game.Engine.Core.SystemActors.Royale
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;

    public class StartingBlock : ActorBody, ICollide
    {
        public RoyaleMode ParentGame;

        public StartingBlock()
        {
            Size = 200;
            Sprite = Sprites.ctf_base;
            CausesCollisions = true;
        }

        public override void Init(World world)
        {
            base.Init(world);
            Position = world.RandomPosition();
        }

        void ICollide.CollisionExecute(Body projectedBody)
        {
            ParentGame.StartCountdown();
        }

        bool ICollide.IsCollision(Body projectedBody)
        {
            if (projectedBody is ShipWeaponBullet
                && ParentGame.World.AdvertisedPlayerCount > 1)
                return true;

            return false;
        }
    }
}
