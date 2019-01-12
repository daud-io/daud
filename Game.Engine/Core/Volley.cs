namespace Game.Engine.Core
{
    using Game.API.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Volley : ActorGroup
    {
        public Fleet FiredFrom { get; set; }
        public List<Bullet> NewBullets { get; set; } = new List<Bullet>();
        public List<Bullet> AllBullets { get; set; } = new List<Bullet>();
        public List<Tuple<Ship, long>> FiringSequence = new List<Tuple<Ship, long>>();

        private int Size = 20;
        private bool IsSeeker = false;
        private Sprites? SpriteOverride = null;

        public static void FireFrom(Fleet fleet)
        {
            var volley = new Volley
            {
                FiredFrom = fleet,
                GroupType = GroupTypes.VolleyBullet,
                OwnerID = fleet.ID,
                ZIndex = 50,
                Size = fleet.Pickup?.Size ?? 20,
                SpriteOverride = fleet.Pickup?.BulletSprite,
                IsSeeker = fleet.Pickup != null,
                Color = fleet.Color
            };

            for (var i=0; i<fleet.Ships.Count; i++)
                volley.FiringSequence.Add(
                    new Tuple<Ship, long>(
                        fleet.Ships[i],
                        fleet.World.Time + i * fleet.World.Hook.FiringSequenceDelay
                    )
                );

            volley.Init(fleet.World);
        }

        public override void Think()
        {
            base.Think();

            var fired = new List<Tuple<Ship, long>>();
            foreach (var pair in FiringSequence)
            {
                var ship = pair.Item1;
                var fireBy = pair.Item2;

                if (
                    ship.Exists 
                    && !ship.PendingDestruction 
                    && !ship.Abandoned
                    && ship.Fleet != null
                    && fireBy <= ship.World.Time 
                    && fireBy > 0)
                {
                    var bullet = Bullet.FireFrom(ship, IsSeeker, SpriteOverride, Size);
                    bullet.Group = this;
                    this.NewBullets.Add(bullet);
                    fired.Add(pair);
                }
            }

            FiringSequence = FiringSequence.Except(fired).ToList();
            fired.Clear();

            this.PendingDestruction = 
                this.PendingDestruction
                || (
                    !NewBullets.Any() 
                    && !AllBullets.Any(b => b.Exists)
                );
        }

        public override void CreateDestroy()
        {
            base.CreateDestroy();

            foreach (var bullet in NewBullets)
                bullet.Init(World);

            AllBullets.AddRange(NewBullets);
            NewBullets.Clear();
        }
    }
}