namespace Game.Engine.Core.SystemActors.CTF
{
    using Game.API.Common;
    using System.Numerics;

    public class Flag : WorldBody
    {
        public readonly Team Team;
        private readonly Base Base;

        private ActorGroup FlagGroup;
        public Fleet CarriedBy = null;

        public Flag(World world, Sprites flagSprite, Team team, Base b): base(world)
        {
            Size = 260;
            Team = team;
            Base = b;
            Sprite = flagSprite;

            FlagGroup = new ActorGroup(world);
            FlagGroup.ZIndex = 200;
            this.Group = FlagGroup;
            Position = world.RandomPosition();

        }
        public override void Destroy()
        {
            base.Destroy();
            FlagGroup.Destroy();
        }

        protected override void Update()
        {

            if (!(CarriedBy?.PendingDestruction ?? true))
            {
                this.Position = CarriedBy.FleetCenter;
                this.LinearVelocity = CarriedBy.FleetVelocity;

                //Console.WriteLine($"X:{CarriedBy.FleetMomentum.X} Y:{CarriedBy.FleetMomentum.Y}");
            }
            else
            {
                if (CarriedBy != null)
                {
                    CarriedBy.Burden = 0;
                }

                CarriedBy = null;
                this.LinearVelocity = new Vector2(0, 0);

                if (World.DistanceOutOfBounds(this.Position) > 0 &&
                    this.Position != Vector2.Zero)
                    this.LinearVelocity = Vector2.Normalize(-this.Position) * 0.1f;
                else
                    this.LinearVelocity = Vector2.Zero;
            }

            base.Update();
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

        public override void CollisionExecute(WorldBody otherObject)
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

            base.CollisionExecute(otherObject);
        }
    }
}
