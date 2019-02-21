namespace Game.Engine.Core.SystemActors.CTF
{
    using Game.API.Common;
    using System.Numerics;

    public class Flag : ActorBody
    {
        public readonly Team Team;
        private readonly Base Base;

        private ActorGroup FlagGroup = new ActorGroup();
        public Fleet CarriedBy = null;

        public Flag(Sprites flagSprite, Team team, Base b)
        {
            Size = 200;
            Team = team;
            Base = b;
            Sprite = flagSprite;
            CausesCollisions = true;
        }

        public override void Init(World world)
        {
            base.Init(world);

            FlagGroup.Init(world);
            FlagGroup.ZIndex = 200;
            this.Group = FlagGroup;
            Position = world.RandomPosition();
        }

        public override void Destroy()
        {
            base.Destroy();
            FlagGroup.Destroy();
        }

        public override void Think()
        {
            base.Think();

            if (!(CarriedBy?.PendingDestruction ?? true))
            {
                this.Position = CarriedBy.FleetCenter;
                this.Momentum = CarriedBy.FleetMomentum;

                //Console.WriteLine($"X:{CarriedBy.FleetMomentum.X} Y:{CarriedBy.FleetMomentum.Y}");
            }
            else
            {
                if (CarriedBy != null)
                {
                    CarriedBy.Burden = 0;
                }

                CarriedBy = null;
                this.Momentum = new Vector2(0, 0);

                if (World.DistanceOutOfBounds(this.Position) > 0 &&
                    this.Position != Vector2.Zero)
                    this.Momentum = Vector2.Normalize(-this.Position) * 0.1f;
                else
                    this.Momentum = Vector2.Zero;
            }
        }

        public void ReturnToBase()
        {
            if (CarriedBy != null)
            {
                CarriedBy.Burden = 0;
            }

            this.Position = Base.Position;
            this.CarriedBy = null;
        }

        protected override void Collided(ICollide otherObject)
        {
            if (otherObject is Ship ship)
            {
                var fleet = ship.Fleet;

                if (CarriedBy == null && fleet != null && !(fleet.Owner is Robot))
                {
                    if (fleet.Owner.Color == Team.ColorName)
                    {
                        ReturnToBase();
                    }
                    else
                    {
                        CarriedBy = fleet;

                        if (CarriedBy != null)
                        {
                            CarriedBy.Burden = World.Hook.CTFCarryBurden;
                        }
                    }
                }
            }

            base.Collided(otherObject);
        }
    }
}
