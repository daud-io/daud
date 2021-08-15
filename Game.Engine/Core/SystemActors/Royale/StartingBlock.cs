namespace Game.Engine.Core.SystemActors.Royale
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;
    using System.Numerics;

    public class StartingBlock : WorldBody
    {
        public RoyaleMode ParentGame;

        public StartingBlock(World world): base(world)
        {
            Size = 200;
            AngularVelocity = 0.01f;
            Sprite = Sprites.ctf_base;
            Position = world.RandomPosition();
        }

        public override void CollisionExecute(WorldBody projectedBody)
        {
            ParentGame.StartCountdown();
        }

        public override bool IsCollision(WorldBody otherBody)
        {
            if (otherBody is ShipWeaponBullet bullet)
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
