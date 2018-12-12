namespace Game.Engine.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public class Volley : ActorGroup
    {
        public Fleet FiredFrom { get; set; }
        public List<Bullet> NewBullets { get; set; } = new List<Bullet>();
        public List<Bullet> AllBullets { get; set; } = new List<Bullet>();

        public static void FireFrom(Fleet fleet)
        {
            var volley = new Volley
            {
                FiredFrom = fleet,
                GroupType = GroupTypes.VolleyBullet,
                OwnerID = fleet.ID,
                ZIndex = 50
            };

            foreach (var ship in fleet.Ships)
            {
                var bullet = Bullet.FireFrom(ship);
                bullet.Group = volley;
                volley.NewBullets.Add(bullet);
            }

            volley.Init(fleet.World);
        }

        public override void Think()
        {
            base.Think();

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