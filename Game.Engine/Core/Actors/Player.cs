namespace Game.Engine.Core.Actors
{
    using Game.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class Player : ActorBase
    {
        public string Name { get; set; } = null;
        public string Ship { get; set; } = null;
        public Vector2 Thrust { get; set; } = new Vector2(0,0);
        public PlayerView View { get; set; } = null;
        public GameObject GameObject { get; set; } = null;
        public float Angle { get; set; } = 0;

        public bool BoostRequested { get; set; } = false;
        public bool ShootRequested { get; set; } = false;

        public virtual int ShootCooldownTime { get => world.Hook.ShootCooldownTime; }
        public virtual int MaxBoostTime { get => world.Hook.MaxBoostTime; }

        public virtual int BaseThrust { get => world.Hook.BaseThrust; }
        public virtual int MaxSpeed { get => world.Hook.MaxSpeed; }
        public virtual int MaxSpeedBoost { get => world.Hook.MaxSpeedBoost; }

        public long ShootCooldown { get; set; } = 0;

        public long BoostTimer { get; set; } = 100;

        public int Score { get; set; } = 0;

        public virtual float HealthRegenerationPerFrame { get => world.Hook.HealthRegenerationPerFrame; }
        public virtual float MaxHealth { get => world.Hook.MaxHealth; }
        public float Health { get; set; } = 0;
        public virtual float HealthHitCost { get => world.Hook.HealthHitCost; }
        public bool IsAlive { get; set; } = false;

        public GameObject Killer { get; set; }

        public List<string> Messages = new List<string>();

        public override void Step()
        {
            bool isBoosting = BoostRequested;

            BoostTimer += isBoosting ? -1 : 3;
            if(BoostTimer < 0) {
                isBoosting = false;
                BoostTimer = 0;
            }
            if(BoostTimer > MaxBoostTime) {
                BoostTimer = MaxBoostTime;
            }

            Health = Math.Min(Health, MaxHealth);
            Health = Math.Max(Health, 0);

            GameObject.Size = (int)(60 + (Health / MaxHealth) * 90);

            if (IsAlive)
            {
                bool isShooting = ShootRequested && ShootCooldown < world.Time;

                // calculate a thrust vector from steering
                float thrustAmount = BaseThrust;

                if (isBoosting)
                    thrustAmount *= 2;

                Thrust =
                    Vector2.Transform(
                        new Vector2(thrustAmount, 0),
                        Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), Angle)
                    );

                float speedLimit = isBoosting
                    ? MaxSpeedBoost
                    : MaxSpeed;

                var x = Vector2.Add(GameObject.Momentum, Thrust);
                var currentSpeed = Math.Abs(Vector2.Distance(x, Vector2.Zero));
                if (currentSpeed > speedLimit)
                    x = Vector2.Multiply(Vector2.Normalize(x), ((speedLimit+3*currentSpeed)/4));

                if (isShooting)
                {
                    ShootCooldown = world.Time + ShootCooldownTime;

                    var bulletSpeed = world.Hook.BulletSpeed;
                    var bulletMomentum = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle)) * bulletSpeed;

                    var bullet = new Bullet(new Vector2(GameObject.Position.X, GameObject.Position.Y), bulletMomentum, Angle)
                    {
                        Owner = this
                    };
                    bullet.Init(world);
                }

                Health = Math.Min(Health + HealthRegenerationPerFrame, MaxHealth);
                GameObject.Momentum = x;
                GameObject.Angle = Angle;
            }

            GameObject.Caption = Name;
            GameObject.Sprite = Ship;
            GameObject.Health = Health / MaxHealth;
        }

        public void SendMessage(string message)
        {
            Messages.Add(message);
        }

        public virtual void Hit(Bullet bullet)
        {
            Health -= HealthHitCost;

            if (Health <= 0 && IsAlive)
            {
                Die();

                Random r = new Random();

                bullet.Owner.Score += 55;

                this.Killer = bullet.Owner.GameObject;

                bullet.Owner.SendMessage($"You Killed {this.Name} - ${r.Next()}");
                this.SendMessage($"Killed by {bullet.Owner.Name}");
            }
        }

        public void Spawn()
        {
            if (IsAlive) return;

            var r = new Random();

            GameObject = new GameObject
            {
                Position = new Vector2
                {
                    X = r.Next(-1000, 1000),
                    Y = r.Next(-1000, 1000)
                },
                Momentum = new Vector2
                {
                    X = 0,
                    Y = 0
                },
                ObjectType = "player"
            };

            Health = MaxHealth;

            world.Objects.Add(GameObject);
            IsAlive = true;
        }

        public override void Init(World world)
        {
            base.Init(world);
            world.Players.Add(this);
        }

        public override void Deinit()
        {
            Die();
            world.Players.Remove(this);
            base.Deinit();
        }

        public virtual void Die()
        {
            IsAlive = false;

            Score /= 2;

            if (GameObject != null
                && world.Objects.Contains(GameObject))
                world.Objects.Remove(GameObject);
        }


        public override void PostStep()
        {
            var leader = world.Players.OrderByDescending(p => p.Score).FirstOrDefault(p => p.IsAlive);

            var v = new PlayerView
            {
                Time = world.Time,
                PlayerCount = world.PlayerCount,

                Objects = world.Objects.Select(o => new GameObject
                {
                    Angle = o.Angle,
                    LastPosition = o.LastPosition,
                    Momentum = o.Momentum,
                    ObjectType = o.ObjectType,
                    Position = o.Position,
                    Caption = false && this.GameObject == o
                        ? null
                        : o.Caption,
                    Sprite = o.Sprite,
                    Health = o.Health
                }).ToArray(),

                Position = IsAlive 
                    ? GameObject?.Position ?? new Vector2(0,0)
                    : leader?.GameObject?.Position ?? new Vector2(0, 0),
                LastPosition = IsAlive
                    ? GameObject?.LastPosition ?? new Vector2(0, 0)
                    : leader?.GameObject?.LastPosition ?? new Vector2(0, 0),
                Momentum = IsAlive
                    ? GameObject?.Momentum ?? new Vector2(0, 0)
                    : new Vector2(0, 0),
                Leaderboard = world.IsLeaderboardNew
                    ? world.Leaderboard
                    : null,
                IsAlive = IsAlive,
                Messages = Messages.ToList(),
                Hook = world.FrameNumber % 20 == 0
                    ? world.Hook
                    : null
            };

            Messages.Clear();

            View = v;
        }
    }
}
