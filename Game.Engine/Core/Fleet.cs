namespace Game.Engine.Core
{
    using System;
    using System.Numerics;

    public class Fleet : ActorBody
    {
        public virtual int ShootCooldownTime { get => World.Hook.ShootCooldownTime; }
        public virtual int BaseThrust { get => World.Hook.BaseThrust; }
        public virtual int MaxSpeed { get => World.Hook.MaxSpeed; }
        public virtual int MaxSpeedBoost { get => World.Hook.MaxSpeedBoost; }
        public virtual int MaxHealth { get => World.Hook.MaxHealth; }
        public virtual float HealthRegenerationPerFrame { get => World.Hook.HealthRegenerationPerFrame; }

        public Player Owner { get; set; }

        public float Health { get; set; }

        public int SizeMinimum { get; set; }
        public int SizeMaximum { get; set; }

        public bool BoostRequested { get; set; }
        public bool ShootRequested { get; set; }

        public long TimeReloaded { get; set; } = 0;

        public override void Step()
        {
            Health = Math.Max(Math.Min(Health, MaxHealth), 0);

            Size = (int)(SizeMinimum + (Health / MaxHealth) * (SizeMaximum-SizeMinimum));

            var isShooting = ShootRequested && World.Time >= TimeReloaded;
            var isBoosting = BoostRequested;

            float thrustAmount = BaseThrust / 40.0f;

            if (isBoosting)
                thrustAmount *= 2;

            var Thrust =
                Vector2.Transform(
                    new Vector2(thrustAmount, 0),
                    Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), Angle)
                );

            float speedLimit = isBoosting
                ? MaxSpeedBoost / 40
                : MaxSpeed / 40;

            var x = Vector2.Add(this.Momentum, Thrust);
            var currentSpeed = Math.Abs(Vector2.Distance(x, Vector2.Zero));
            if (currentSpeed > speedLimit)
                x = Vector2.Multiply(Vector2.Normalize(x), ((speedLimit + 3 * currentSpeed) / 4));

            if (!Momentum.Equals(x))
            {
                Momentum = x;
                DefinitionTime = World.Time;
            }

            if (isShooting)
            {
                TimeReloaded = World.Time + ShootCooldownTime;

                Bullet.FireFrom(this);
            }

            Health = Math.Min(Health + HealthRegenerationPerFrame, MaxHealth);
        }
    }   
}
