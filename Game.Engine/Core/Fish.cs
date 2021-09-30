namespace Game.Engine.Core
{
    using Game.API.Common;
    using Game.Engine.Core.Steering;
    using System;
    using System.Linq;
    using System.Numerics;

    public class Fish : Ship
    {
        public Fish(World world) : base(world)
        {
            this.Mass = 10;
            
            Size = 70;
            Sprite = Sprites.fish;
            CycleMS = World.Hook.FishCycle;
            
            Position = World.ChooseSpawnPoint("fish", this);
            Angle = (float)World.Random.NextDouble() * MathF.PI * 2;
            ThrustAmount = World.Hook.FishThrust;
        }

        protected override void Update(float dt)
        {
            base.Update(dt);
            Flock();
        }

        private void Flock()
        {
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

            var steeringVector =
                new Vector2(MathF.Cos(Angle), MathF.Sin(Angle))
                + World.Hook.FishFlockWeight * flockingVector
                + oobVector;

            Angle = MathF.Atan2(steeringVector.Y, steeringVector.X);
            AngularVelocity = 0;
        }
    }
}
