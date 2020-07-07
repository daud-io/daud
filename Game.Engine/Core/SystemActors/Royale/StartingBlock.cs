namespace Game.Engine.Core.SystemActors.Royale
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;
    using System.Numerics;

    public class StartingBlock : ActorBody, ICollide
    {
        public RoyaleMode ParentGame;

        public StartingBlock()
        {
            Size = 200;
            AngularVelocity = 0.01f;
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
            if (projectedBody is ShipWeaponBullet bullet)
            {
                if (ParentGame.World.AdvertisedPlayerCount > 1)
                    return true;

                var player = bullet?.OwnedByFleet?.Owner;

                if (player != null)
                    player.SendMessage("Wait for at least 2 players, then shoot this thing to start.");
            }

            return false;
        }
    }
}
