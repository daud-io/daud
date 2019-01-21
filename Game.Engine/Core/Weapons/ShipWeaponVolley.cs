namespace Game.Engine.Core.Weapons
{
    using Game.API.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ShipWeaponVolley<T> : ActorGroup
        where T: IShipWeapon, new()
    {
        public Fleet FiredFrom { get; set; }
        public List<IShipWeapon> NewWeapons { get; set; } = new List<IShipWeapon>();
        public List<IShipWeapon> AllWeapons { get; set; } = new List<IShipWeapon>();
        public List<Tuple<Ship, long>> FiringSequence = new List<Tuple<Ship, long>>();
        private Action<IShipWeapon> Configure;

        public static void FireFrom(Fleet fleet, Action<IShipWeapon> configure = null)
        {
            var volley = new ShipWeaponVolley<T>
            {
                Configure = configure,
                FiredFrom = fleet,
                GroupType = GroupTypes.VolleyBullet,
                OwnerID = fleet.ID,
                ZIndex = 50,
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

                    var shipWeapon = new T();
                    shipWeapon.FireFrom(ship, this);
                    Configure?.Invoke(shipWeapon);

                    this.NewWeapons.Add(shipWeapon);
                    fired.Add(pair);
                }
            }

            FiringSequence = FiringSequence.Except(fired).ToList();
            fired.Clear();

            this.PendingDestruction = 
                this.PendingDestruction
                || (
                    !NewWeapons.Any() 
                    && !AllWeapons.Any(b => b.Active)
                );
        }

        public override void CreateDestroy()
        {
            base.CreateDestroy();

            foreach (var bullet in NewWeapons)
                bullet.Init(World);

            AllWeapons.AddRange(NewWeapons);
            NewWeapons.Clear();
        }
    }
}