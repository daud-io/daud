namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Steering;
    using System;
    using System.Linq;
    using System.Numerics;

    public class Fish : Ship
    {
        private long SleepUntil = 0;

        public Fish()
        {
            Size = 10;
            Sprite = Sprites.fish;
            Color = "rgba(128,128,128,.2)";
        }

        public override void Init(World world)
        {
            World = world;
            Randomize();
            base.Init(world);
        }

        public void Randomize()
        {
            var r = new Random();
            Position = World.RandomPosition();
            Angle = (float)r.NextDouble() * MathF.PI * 2;
            ThrustAmount = World.Hook.FishThrust;
        }

        public override void Think()
        {
            if (SleepUntil < World.Time)
            {
                Flock();
                base.Think();
                SleepUntil = World.Time + World.Hook.FishCycle;
            }
        }

        private void Flock()
        {
            var oobVectorWeight = 0.8f;
            var ships = World.BodiesNear(Position, World.Hook.FishFlockCohesionMaximumDistance)
                .OfType<Ship>();

            var flockingVector = Vector2.Zero;
            var oobVector = Vector2.Zero;

            if (ships.Count() > 1)
                flockingVector =
                    (World.Hook.FishFlockCohesion
                        * Flocking.Cohesion(ships, this, World.Hook.FishFlockCohesionMaximumDistance))
                    + (World.Hook.FishFlockAlignment
                        * Flocking.Alignment(ships, this))
                    + (World.Hook.FishFlockSeparation
                        * Flocking.Separation(ships, this, World.Hook.FishFlockSeparationMinimumDistance));

            if (IsOOB)
            {
                if (Position != Vector2.Zero)
                    oobVector = Vector2.Normalize(-Position) * oobVectorWeight;
            }

            var steeringVector =
                new Vector2(MathF.Cos(Angle), MathF.Sin(Angle))
                + World.Hook.FishFlockWeight * flockingVector
                + oobVector;

            Angle = MathF.Atan2(steeringVector.Y, steeringVector.X);
        }
    }
}
