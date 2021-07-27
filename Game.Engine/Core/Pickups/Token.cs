namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;
    using System.Numerics;

    public class Token : ActorBody
    {
        private ActorGroup TokenGroup = new ActorGroup();
        public float Burden = -0.2f;
        public Fleet CarriedBy = null;

        public Token()
        {
            Size = 200;

            Sprite = Sprites.haste_powerup;
            CausesCollisions = true;
        }

        public override void Init(World world)
        {
            base.Init(world);

            TokenGroup.Init(world);
            TokenGroup.ZIndex = 300;
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

                // relieve the carrier of it
                if (CarriedBy != null)
                    CarriedBy.Burden = 0;
                CarriedBy = null;

                
                if (World.DistanceOutOfBounds(this.Position) > 0 &&
                    this.Position != Vector2.Zero)
                    this.Momentum = Vector2.Normalize(-this.Position) * 0.1f;
                else
                    this.Momentum = Vector2.Zero;
            }
        }

        protected override void Collided(ICollide otherObject)
        {
            if (otherObject is Ship ship)
            {
                var fleet = ship.Fleet;

                if (CarriedBy == null && fleet != null && !(fleet.Owner is Robot))
                {
                    CarriedBy = fleet;
                    if (CarriedBy != null)
                        CarriedBy.Burden = Burden;
                }
            }

            base.Collided(otherObject);
        }
        
    }
}
