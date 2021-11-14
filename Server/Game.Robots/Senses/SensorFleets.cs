namespace Game.Robots.Senses
{
    using Game.API.Common;
    using Game.Robots.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class SensorFleets : ISense
    {
        private readonly ContextRobot Robot;

        public List<Fleet> AllVisibleFleets { get; private set; }

        public Fleet MyFleet { get; private set; }
        public Vector2 LastKnownCenter { get; set; }

        public Fleet[] Others { get; set; } = Array.Empty<Fleet>();

        public SensorFleets(ContextRobot robot)
        {
            this.Robot = robot;
            this.AllVisibleFleets = new List<Fleet>();
        }

        public Fleet ByID(uint fleetID)
        {
            return AllVisibleFleets.FirstOrDefault(f => f.ID == fleetID);
        }

        public void Sense()
        {
            foreach (var fleet in AllVisibleFleets)
                fleet.PendingDestruction = true;
            
            var fleetGroups = Robot.Bodies
                .Where(b => b.Group?.Type == GroupTypes.Fleet) // check the sprite
                .GroupBy(keySelector: b => b.Group);

            foreach (var ships in fleetGroups)
            {
                var fleetGroup = ships.Key;
                var fleet = AllVisibleFleets.FirstOrDefault(f => f.ID == fleetGroup.ID);
                if (fleet == null)
                {
                    fleet = new Fleet
                    {
                        ID = fleetGroup.ID
                    };
                    AllVisibleFleets.Add(fleet);
                }

                fleet.Name = fleetGroup.Caption;
                fleet.Color = fleetGroup.Color;
                fleet.PendingDestruction = false;

                foreach (var ship in fleet.Ships)
                    ship.PendingDestruction = true;

                foreach (var shipBody in ships)
                {
                    var ship = fleet.Ships.FirstOrDefault(s => s.ID == shipBody.ID);
                    if (ship == null)
                    {
                        ship = new Ship();
                        ship.ID = shipBody.ID;
                        fleet.Ships.Add(ship);
                    }

                    ship.Position = shipBody.Position;
                    ship.Momentum = shipBody.Velocity;
                    ship.Size = shipBody.Size;
                    ship.Angle = shipBody.Angle;
                    ship.PendingDestruction = false;
                }

                // destruction is nigh
                fleet.Ships = fleet.Ships.Where(s => !s.PendingDestruction).ToList();
            }

            // destruction is nigh
            AllVisibleFleets = AllVisibleFleets.Where(f => !f.PendingDestruction).ToList();

            foreach (var fleet in AllVisibleFleets)
                fleet.CacheCenter();

            MyFleet = AllVisibleFleets.FirstOrDefault(f => f.ID == Robot.FleetID);

            if (AllVisibleFleets.Count(f => f.ID == Robot.FleetID) > 1)
            {
                Console.WriteLine($"Multiple fleets with ID: {Robot.FleetID}");
            }

            if (MyFleet != null)
                LastKnownCenter = MyFleet.Center;

            Others = MyFleet != null
                ? AllVisibleFleets.Where(v => v != MyFleet).ToArray()
                : AllVisibleFleets.ToArray();
        }
    }
}
