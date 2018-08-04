namespace Game.Engine.Core
{
    using Game.Models;
    using System;
    using System.Linq;
    using System.Numerics;

    public class Player
    {
        public string Name { get; set; } = null;
        public string Ship { get; set; } = null;
        public Vector2 Thrust { get; set; } = new Vector2(0,0);
        public PlayerView View { get; set; } = null;
        public GameObject GameObject { get; set; } = null;
        public float Angle { get; set; } = 0;

        public bool BoostRequested { get; set; } = false;
        public bool ShootRequested { get; set; } = false;

        private const int SHOOT_COOLDOWN_TIME = 500;
        private const int MAX_BOOST_TIME = 100;

        public long ShootCooldown { get; set; } = 0;

        public long BoostTimer { get; set; } = 100;

        public int Score { get; set; } = 0;

        public float HealthRegenerationPerFrame { get; set; } = 0.3f;
        public float MaxHealth { get; set; } = 100;
        public float Health { get; set; } = 0;
        public float HealthHitCost { get; set; } = 20;
        public bool IsAlive { get; set; } = false;

        public GameObject Killer { get; set; }

        protected readonly World world;

        public Player(World world)
        {
            this.world = world;
        }

        public virtual void Step()
        {
            bool isBoosting = BoostRequested;

            BoostTimer += isBoosting ? -1 : 3;
            if(BoostTimer < 0) {
                isBoosting = false;
                BoostTimer = 0;
            }
            if(BoostTimer > MAX_BOOST_TIME) {
                BoostTimer = MAX_BOOST_TIME;
            }

            Health = Math.Min(Health, MaxHealth);
            Health = Math.Max(Health, 0);

            if (IsAlive)
            {
                bool isShooting = ShootRequested && ShootCooldown < world.Time;

                // calculate a thrust vector from steering
                float thrustAmount = 6;

                if (isBoosting)
                    thrustAmount *= 2;

                Thrust =
                    Vector2.Transform(
                        new Vector2(thrustAmount, 0),
                        Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), Angle)
                    );


                float boostSpeed = 40;

                float speedLimit = isBoosting
                    ? boostSpeed
                    : 12;

                var x = Vector2.Add(GameObject.Momentum, Thrust);
                var currentSpeed = Math.Abs(Vector2.Distance(x, Vector2.Zero));
                if (currentSpeed > speedLimit)
                    x = Vector2.Multiply(Vector2.Normalize(x), ((speedLimit+3*currentSpeed)/4));

                if (isShooting)
                {
                    ShootCooldown = world.Time + SHOOT_COOLDOWN_TIME;

                    var bulletSpeed = 70;
                    var bulletMomentum = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle)) * bulletSpeed;

                    var bullet = new Bullet(world, new Vector2(GameObject.Position.X, GameObject.Position.Y), bulletMomentum, Angle)
                    {
                        Owner = this
                    };
                }

                Health = Math.Min(Health + HealthRegenerationPerFrame, MaxHealth);
                GameObject.Momentum = x;
                GameObject.Angle = Angle;
            }

            GameObject.Caption = Name;
            GameObject.Sprite = Ship;
            GameObject.Health = Health / MaxHealth;
        }

        public virtual void Hit(Bullet bullet)
        {
            Health -= HealthHitCost;

            if (Health <= 0)
            {
                Die();

                bullet.Owner.Score += 55;
                this.Killer = bullet.Owner.GameObject;
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

            world.Objects.Add(GameObject);
            IsAlive = true;
        }

        public virtual void Init()
        {
            world.Players.Add(this);
        }

        public virtual void Die()
        {
            IsAlive = false;

            Score /= 2;

            if (GameObject != null
                && world.Objects.Contains(GameObject))
                world.Objects.Remove(GameObject);
        }

        public virtual void Deinit()
        {
            Die();
            world.Players.Remove(this);
        }

        public virtual void SetupView()
        {
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
                    : Killer?.Position ?? new Vector2(0, 0),
                LastPosition = IsAlive
                    ? GameObject?.LastPosition ?? new Vector2(0, 0)
                    : Killer?.LastPosition ?? new Vector2(0, 0),
                Momentum = IsAlive
                    ? GameObject?.Momentum ?? new Vector2(0, 0)
                    : new Vector2(0, 0),
                Leaderboard = world.IsLeaderboardNew
                    ? world.Leaderboard
                    : null,
                IsAlive = IsAlive
            };

            View = v;
        }
    }
}
