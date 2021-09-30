﻿namespace Game.Engine.Core.SystemActors.Royale
{
    using Game.API.Common;
    using Game.Engine.Core.Weapons;
    using System.Numerics;

    public class StartingBlock : WorldBody
    {
        public RoyaleMode ParentGame;

        public StartingBlock(World world) : base(world)
        {
            Size = 200;
            AngularVelocity = 0.01f;
            Sprite = Sprites.ctf_base;
            Position = world.RandomPosition();
        }

        public override void CollisionExecute(WorldBody projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
            {

                if (ParentGame.World.AdvertisedPlayerCount > 1)
                    ParentGame.StartCountdown();
                else
                {
                    var player = bullet?.OwnedByFleet?.Owner;
                    if (player != null)
                        player.SendMessage("Wait for at least 2 players, then shoot this thing to start.");
                }
            }
        }

        public override CollisionResponse CanCollide(WorldBody otherBody)
        {
            CollisionResponse response = new CollisionResponse();

            if (otherBody is ShipWeaponBullet)
                response.CanCollide = true;

            return response;
        }
    }
}
