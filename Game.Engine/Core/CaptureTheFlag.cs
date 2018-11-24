namespace Game.Engine.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class CaptureTheFlag : IActor
    {
        private World World = null;
        private List<Flag> Flags = new List<Flag>();
        private List<Base> Bases = new List<Base>();

        void IActor.CreateDestroy()
        {
            if (World.Hook.CTFMode && Flags.Count == 0)
            {
                Flags.Add(new Flag("flag_blue"));
                Flags.Add(new Flag("flag_red"));

                foreach (var flag in Flags)
                    flag.Init(World);
            }

            if (!World.Hook.CTFMode && Flags.Count > 0)
            {
                foreach (var flag in Flags)
                    flag.Destroy();
            }

        }

        void IActor.Destroy()
        {
            World.Actors.Remove(this);
        }

        void IActor.Init(World world)
        {
            World = world;
            World.Actors.Add(this);
        }

        void IActor.Think()
        {
        }

        private class Base : ActorBody, ICollide
        {
            void ICollide.CollisionExecute(Body projectedBody)
            {
                throw new NotImplementedException();
            }

            bool ICollide.IsCollision(Body projectedBody)
            {
                throw new NotImplementedException();
            }
        }

        private class Flag : ActorBody, ICollide
        {
            private List<Sprites> SpriteSet = new List<Sprites>();
            private uint NextSpriteTime = 0;
            private uint SpriteInterval = 100;
            private int SpriteIndex = 0;
            private Fleet CarriedBy = null;

            public Flag(string baseSpriteName)
            {
                Size = 200;

                var i = 0;
                var done = false;

                while (!done)
                {
                    if (Enum.TryParse<Sprites>($"{baseSpriteName}_{i++}", out var result))
                        SpriteSet.Add(result);
                    else
                        done = true;
                }

                if (SpriteSet.Any())
                    Sprite = SpriteSet[0];
            }

            public override void Init(World world)
            {
                base.Init(world);
                this.Position = world.RandomPosition();
            }

            public override void Think()
            {
                base.Think();

                if (CarriedBy?.Owner?.IsAlive ?? false)
                {
                    this.Position = CarriedBy.FleetCenter;
                    this.Momentum = CarriedBy.FleetMomentum;
                }
                else
                {
                    CarriedBy = null;
                    this.Momentum = new Vector2(0, 0);
                }

                if (World.Time > NextSpriteTime)
                {
                    SpriteIndex = (SpriteIndex + 1) % SpriteSet.Count;

                    Sprite = SpriteSet[SpriteIndex];

                    NextSpriteTime = World.Time + SpriteInterval;
                }
                
            }

            void ICollide.CollisionExecute(Body projectedBody)
            {
                var ship = projectedBody as Ship;
                var fleet = ship.Fleet;

                if (fleet != null && CarriedBy == null && !(fleet.Owner is Robot))
                {
                    CarriedBy = fleet;
                }
            }

            bool ICollide.IsCollision(Body projectedBody)
            {
                if (projectedBody is Ship ship)
                {
                    if (ship.Abandoned)
                        return false;

                    return Vector2.Distance(projectedBody.Position, this.Position)
                        < (projectedBody.Size + this.Size);
                }
                return false;
            }
        }
    }
}
