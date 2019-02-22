namespace Game.Engine.Core.SystemActors.CTF
{
    using Game.API.Common;
    using System.Numerics;

    public class Base : ActorBody, ICollide
    {
        private readonly Team Team;
        public Flag Flag { get; set; }
        private const float SPEED_SPINNING = 0.001f;
        private const float SPEED_STOPPED = 0f;
        private readonly CaptureTheFlag CaptureTheFlag = null;
        public ActorGroup BaseGroup;

        public Base(CaptureTheFlag captureTheFlag, Vector2 position, Team team)
        {
            this.Team = team;
            this.Position = position;
            this.Sprite = Sprites.ctf_base;
            this.AngularVelocity = SPEED_STOPPED;
            this.Size = 200;
            this.CaptureTheFlag = captureTheFlag;
            CausesCollisions = true;

            this.BaseGroup = new ActorGroup();

            BaseGroup.Init(captureTheFlag.World);
            this.Group = BaseGroup;
        }

        public override void Think()
        {
            base.Think();

            this.AngularVelocity = FlagIsHome()
                ? SPEED_SPINNING
                : SPEED_STOPPED;
        }

        void ICollide.CollisionExecute(Body projectedBody)
        {
            var flag = projectedBody as Flag;

            this.Team.Scored();

            flag.ReturnToBase();
        }

        public bool FlagIsHome()
        {
            return Vector2.Distance(Flag.Position, this.Position)
                < (Flag.Size + this.Size);
        }

        bool ICollide.IsCollision(Body projectedBody)
        {
            if (projectedBody is Flag flag)
            {
                if (flag.Team == this.Team)
                {
                    return false;
                }

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

                    return false;
                }

                return Vector2.Distance(projectedBody.Position, this.Position)
                        < (projectedBody.Size + this.Size);
            }
            return false;
        }

        public override void Destroy()
        {
            base.Destroy();
            this.BaseGroup.Destroy();
        }
    }
}
