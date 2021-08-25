namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using Newtonsoft.Json;
    using System;
    using System.Numerics;

    public class Token : WorldBody
    {
        private ActorGroup TokenGroup;
        public Fleet CarriedBy = null;

        public bool ExpiringSoon = false;

        public TokenDataType TokenData { get; set ;}
        

        [Flags]
        public enum TokenModeEnum
        {
            none = 0,
            carried = 1,
            expiring = 2
        }

        public Token(World world): base(world)
        {
            Size = 200;

            Sprite = Sprites.haste_powerup;
            Mode = 0;

            TokenGroup = new ActorGroup(world);
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

        protected override void Update()
        {

            bool carried = 
                    CarriedBy != null
                &&  !CarriedBy.PendingDestruction;

            if (carried)
            {
                this.Position = CarriedBy.FleetCenter;
                this.LinearVelocity = CarriedBy.FleetVelocity;
            }
            else
            {
                // not carried anymore
                if (CarriedBy != null)
                    this.Drop();
                
                if (World.DistanceOutOfBounds(this.Position) > 0 &&
                    this.Position != Vector2.Zero)
                    this.LinearVelocity = Vector2.Normalize(-this.Position) * 0.1f;
                else
                    this.LinearVelocity = Vector2.Zero;
            }

            this.Mode = (byte)
                (
                    (carried ? TokenModeEnum.carried : TokenModeEnum.none)
                    | (this.ExpiringSoon ? TokenModeEnum.expiring : TokenModeEnum.none)
                );

            base.Update();
        }

        protected void Drop()
        {
            this.OnDroppedByFleet(CarriedBy);
            this.SetFleetID(null);
            CarriedBy = null;
        }

        public override CollisionResponse CanCollide(WorldBody otherObject)
        {
            if (otherObject is Ship ship)
            {
                var fleet = ship.Fleet;

                if (CarriedBy == null && fleet != null)
                    return new CollisionResponse(true, false);
            }

            return base.CanCollide(otherObject);
        }
        public override void CollisionExecute(WorldBody otherObject)
        {
            if (otherObject is Ship ship)
            {
                var fleet = ship.Fleet;

                if (CarriedBy == null && fleet != null)
                {
                    CarriedBy = fleet;
                    this.SetFleetID(CarriedBy.ID);
                    this.OnPickedUpByFleet(fleet);
                }
            }

            base.CollisionExecute(otherObject);
        }

        protected void SetFleetID(uint? id)
        {
            this.TokenData ??= new TokenDataType();
            this.TokenData.FleetID = id;
            this.Group.CustomData = JsonConvert.SerializeObject(this.TokenData);
        }

        protected virtual void OnPickedUpByFleet(Fleet fleet)
        {
        }

        protected virtual void OnDroppedByFleet(Fleet fleet)
        {
        }

        public class TokenDataType
        {
            public uint? FleetID { get; set; }
        }
    }
}
