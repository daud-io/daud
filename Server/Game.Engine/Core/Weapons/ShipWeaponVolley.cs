namespace Game.Engine.Core.Weapons
{
    using Game.API.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ShipWeaponVolley<T> : ActorGroup
        where T : IShipWeapon
    {
        public Fleet FiredFrom { get; set; }
        public List<IShipWeapon> NewWeapons { get; set; } = new List<IShipWeapon>();
        public List<IShipWeapon> AllWeapons { get; set; } = new List<IShipWeapon>();
        public List<Tuple<Ship, long>> FiringSequence = new List<Tuple<Ship, long>>();
        private Action<IShipWeapon> Configure;

        public ShipWeaponVolley(World world): base(world)
        {

        }

        public static void FireFrom(Fleet fleet, Action<IShipWeapon> configure = null)
        {
            var volley = Activator.CreateInstance(typeof(ShipWeaponVolley<T>), fleet.World) as ShipWeaponVolley<T>;
            volley.Configure = configure;
            volley.FiredFrom = fleet;
            volley.GroupType = GroupTypes.VolleyBullet;
            volley.OwnerID = fleet.ID;
            volley.ZIndex = 150;
            volley.Color = fleet.Color;

            for (var i = 0; i < fleet.Ships.Count; i++)
            {
                var shipWeapon = Activator.CreateInstance(typeof(T), fleet.World, fleet.Ships[i]) as IShipWeapon;
                shipWeapon.FireFrom(fleet.Ships[i], volley);
                configure?.Invoke(shipWeapon);
                volley.NewWeapons.Add(shipWeapon);


                // volley.FiringSequence.Add(
                //     new Tuple<Ship, long>(
                //         fleet.Ships[i],
                //         fleet.World.Time + i * fleet.World.Hook.FiringSequenceDelay
                //     )
                // );

            }
        }

        public override void Think(float dt)
        {
            base.Think(dt);

            this.PendingDestruction =
                this.PendingDestruction
                || (
                    !NewWeapons.Any()
                    && !AllWeapons.Any(b => b.Active())
                );


            AllWeapons.AddRange(NewWeapons);
            NewWeapons.Clear();
        }
    }
}