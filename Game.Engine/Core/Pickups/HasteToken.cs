namespace Game.Engine.Core.Pickups
{
    using Game.API.Common;

    public class HasteToken : Token
    {
        // config
        public int EffectSeconds = 120;
        public int CooldownSeconds = 120;
        public float Burden = -0.2f;
        public int SpriteSize = 150;

        // state
        public bool Cooldown = false;
        public uint CooldownUntil = 0;
        public int EffectMSRemaining = 0;

        public HasteToken(World world) : base(world)
        {
            Size = SpriteSize;

            Sprite = Sprites.haste_powerup;
            OnRenewed();
        }

        protected override void Update(float dt)
        {
            if (this.CarriedBy != null)
            {
                this.EffectMSRemaining -= this.World.Hook.StepTime;
                //this.Group.Caption = this.EffectMSRemaining.ToString("0.0");
            }

            if (!Cooldown && EffectMSRemaining <= 0)
            {
                Cooldown = true;
                CooldownUntil = (uint)(World.Time + CooldownSeconds * 1000);
                this.OnCooldownStart();
            }

            if (Cooldown && World.Time > CooldownUntil)
            {
                Cooldown = false;
                this.OnRenewed();
            }

            base.Update(dt);
        }

        protected virtual void OnCooldownStart()
        {
            this.Drop();
            this.Size = 0;
        }

        protected virtual void OnRenewed()
        {
            this.Position = World.RandomPosition();
            this.Size = SpriteSize;
            this.EffectMSRemaining = this.EffectSeconds * 1000;
        }

        protected override void OnPickedUpByFleet(Fleet fleet)
        {
            fleet.Burden = Burden;
        }
        protected override void OnDroppedByFleet(Fleet fleet)
        {
            // relieve the carrier of it
            if (fleet != null)
                fleet.Burden = 0;
        }
    }
}
