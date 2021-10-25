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
            CycleMS = World.Hook.FishCycle + world.Random.Next(World.Hook.FishCycle/2);
            
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

            var flockingVector = Vector2.Zero;

            int count = 0;
            Vector2 exclusiveCenter = Vector2.Zero;
            Vector2 separationAccumulator = Vector2.Zero;
            Vector2 alignmentAccumulator = Vector2.Zero;


            var thisShip = this;
            World.BodiesNear(Position, World.Hook.FishFlockCohesionMaximumDistance, (body) =>
            {
                if (body is Ship otherShip)
                {
                    if (otherShip != thisShip)
                    {
                        count++;

                        var distance = Vector2.Distance(thisShip.Position, otherShip.Position);

                        if (distance < World.Hook.FishFlockCohesionMaximumDistance)
                            exclusiveCenter += otherShip.Position;

                        if (distance < World.Hook.FishFlockSeparationMinimumDistance)
                        {
                            if (distance < 1)
                                distance = 1;

                            separationAccumulator += (thisShip.Position - otherShip.Position) / (distance * distance);
                        }

                        alignmentAccumulator += otherShip.LinearVelocity;
                    }
                }
            });

            Vector2 cohesion = Vector2.Zero;
            Vector2 alignment = Vector2.Zero;
            Vector2 separation = Vector2.Zero;

            if (count > 0)
            {
                exclusiveCenter /= count;
                var relative = exclusiveCenter - thisShip.Position;

                if (relative != Vector2.Zero)
                    cohesion = Vector2.Normalize(relative) * Vector2.Distance(thisShip.Position, exclusiveCenter);

                alignment = alignmentAccumulator / (count);
                separation = separationAccumulator;

                flockingVector =
                    (World.Hook.FishFlockCohesion * cohesion)
                    + (World.Hook.FishFlockAlignment * alignment)
                    + (World.Hook.FishFlockSeparation * separation);

                if (flockingVector != Vector2.Zero)
                {
                    var steeringVector =
                        new Vector2(MathF.Cos(Angle), MathF.Sin(Angle))
                        + flockingVector;

                    Angle = MathF.Atan2(steeringVector.Y, steeringVector.X);
                }
            }
        }
    }
}
