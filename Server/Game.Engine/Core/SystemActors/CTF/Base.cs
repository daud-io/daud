namespace Game.Engine.Core.SystemActors.CTF
{
    using Game.API.Common;
    using System.Numerics;

    public class Base : WorldBody
    {
        private readonly Team Team;
        public Flag Flag { get; set; }
        private const float SPEED_SPINNING = 0.001f;
        private const float SPEED_STOPPED = 0f;
        private readonly CaptureTheFlag CaptureTheFlag = null;
        public ActorGroup BaseGroup;

        public Base(World world, CaptureTheFlag captureTheFlag, Vector2 position, Team team): base(world)
        {
            this.Team = team;
            this.Position = position;
            this.Sprite = Sprites.ctf_base;
            this.AngularVelocity = SPEED_STOPPED;
            this.Size = 200;
            this.CaptureTheFlag = captureTheFlag;

            this.BaseGroup = new ActorGroup(world);
            BaseGroup.ZIndex = 50;

            this.Group = BaseGroup;
        }
        protected override void Update(float dt)
        {
            this.AngularVelocity = FlagIsHome()
                ? SPEED_SPINNING
                : SPEED_STOPPED;

            base.Update(dt);
        }

        public override void CollisionExecute(WorldBody projectedBody)
        {
            if (projectedBody is Flag flag)
            {
                this.Team.Scored();
                flag.ReturnToBase();
            }
        }

        public override CollisionResponse CanCollide(WorldBody projectedBody)
        {
            if (projectedBody is Flag flag)
            {
                if (flag.Team == this.Team)
                    return new CollisionResponse(false);

                if (!FlagIsHome())
                {
                    if (World.Time > CaptureTheFlag.NextAnnounceTime)
                    {
                        var player = flag?.CarriedBy?.Owner;
                        if (player != null)
                        {
                            CaptureTheFlag.NextAnnounceTime = World.Time + 2000;
                            player.SendMessage("Your flag is not home. It must be in your base to score!");
                        }
                    }

                    return new CollisionResponse(false);
                }

                return new CollisionResponse(true);
            }
            else
            {
                if (projectedBody is Ship)
                    return new CollisionResponse(true);
            }
            return new CollisionResponse(false);
        }


        public bool FlagIsHome()
        {
            return Vector2.Distance(Flag.Position, this.Position)
                < (Flag.Size + this.Size);
        }

        public override void Destroy()
        {
            base.Destroy();
            this.BaseGroup.Destroy();
        }
    }
}
