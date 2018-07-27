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

        public void Step(World world)
        {
            bool isBoosting = BoostRequested;

            // calculate a thrust vector from steering
            float thrustAmount = 0.4f;

            if (isBoosting)
                thrustAmount *= 4;

            Thrust =
                Vector2.Transform(
                    new Vector2(thrustAmount, 0),
                    Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), Angle)
                );


            float boostSpeed = 20;

            float speedLimit = isBoosting
                ? boostSpeed
                : 6;

            var x = Vector2.Add(GameObject.Momentum, Thrust);
            var currentSpeed = Math.Abs(Vector2.Distance(x, Vector2.Zero));
            if (currentSpeed > speedLimit)
                x = Vector2.Multiply(Vector2.Normalize(x), ((speedLimit+3*currentSpeed)/4));

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
                    Caption = this.GameObject == o
                        ? null
                        : o.Caption,
                    Sprite = o.Sprite
                }).ToArray(),

                Position = GameObject?.Position,
                LastPosition = GameObject?.LastPosition,
                Momentum = GameObject?.Momentum
            };

            View = v;
        }
    }
}
