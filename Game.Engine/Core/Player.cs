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

        public void Step(World world)
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

                var bullet = new Bullet(world, new Vector2(GameObject.Position.X, GameObject.Position.Y), bulletMomentum, Angle);
                bullet.Owner = this;
            }

            GameObject.Momentum = x;
            GameObject.Angle = Angle;
            GameObject.Caption = Name;
            GameObject.Sprite = Ship;
        }

        public void SetupView(World world)
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
                    Sprite = o.Sprite
                }).ToArray(),

                Position = GameObject?.Position,
                LastPosition = GameObject?.LastPosition,
                Momentum = GameObject?.Momentum,
                Leaderboard = world.IsLeaderboardNew
                    ? world.Leaderboard
                    : null
            };

            View = v;
        }
    }
}
