﻿namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using System;
    using System.Numerics;

    public class Token : ActorBody
    {
        private ActorGroup TokenGroup = new ActorGroup();
        public Fleet CarriedBy = null;

        public bool ExpiringSoon = false;

        [Flags]
        public enum TokenModeEnum
        {
            none = 0,
            carried = 1,
            expiring = 2
        }

        public Token()
        {
            Size = 200;

            Sprite = Sprites.haste_powerup;
            CausesCollisions = true;

            Mode = 0;
        }

        public override void Init(World world)
        {
            base.Init(world);

            TokenGroup.Init(world);
            TokenGroup.ZIndex = 300;
            TokenGroup.GroupType = GroupTypes.Token;
            this.Group = TokenGroup;

            Position = world.RandomPosition();
        }

        public override void Destroy()
        {
            base.Destroy();
            TokenGroup.Destroy();
        }

        public override void Think()
        {
            base.Think();

            bool carried = 
                    CarriedBy != null
                &&  !CarriedBy.PendingDestruction;

            if (carried)
            {
                this.Position = CarriedBy.FleetCenter;
                this.Momentum = CarriedBy.FleetMomentum;
            }
            else
            {
                // not carried anymore
                if (CarriedBy != null)
                    this.Drop();
                
                if (World.DistanceOutOfBounds(this.Position) > 0 &&
                    this.Position != Vector2.Zero)
                    this.Momentum = Vector2.Normalize(-this.Position) * 0.1f;
                else
                    this.Momentum = Vector2.Zero;
            }

            this.Mode = (byte)
                (
                    (carried ? TokenModeEnum.carried : TokenModeEnum.none)
                    | (this.ExpiringSoon ? TokenModeEnum.expiring : TokenModeEnum.none)
                );
        }

        protected void Drop()
        {
            this.OnDroppedByFleet(CarriedBy);
            CarriedBy = null;
        }

        protected override void Collided(ICollide otherObject)
        {
            if (otherObject is Ship ship)
            {
                var fleet = ship.Fleet;

                if (CarriedBy == null && fleet != null && !(fleet.Owner is Robot))
                {
                    CarriedBy = fleet;
                    this.OnPickedUpByFleet(fleet);
                }
            }

            base.Collided(otherObject);
        }

        protected virtual void OnPickedUpByFleet(Fleet fleet)
        {
        }

        protected virtual void OnDroppedByFleet(Fleet fleet)
        {
        }
    }
}
