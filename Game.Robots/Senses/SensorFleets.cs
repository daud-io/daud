namespace Game.Robots.Senses
{
    using Game.API.Common;
    using Game.Robots.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SensorFleets : ISense
    {
        private readonly ContextRobot Robot;

        public List<Fleet> AllVisibleFleets { get; private set; }

        public Fleet MyFleet { get; private set; }

        public SensorFleets(ContextRobot robot)
        {
            this.Robot = robot;
            this.AllVisibleFleets = new List<Fleet>(); ;
        }

        public Fleet ByID(uint fleetID)
        {
            return AllVisibleFleets.FirstOrDefault(f => f.ID == fleetID);
        }

        public void Sense()
        {
            var newFleets = Robot.Bodies
                .Where(b => b.Group?.Type == GroupTypes.Fleet) // check the sprite
                .GroupBy(b => b.Group)
                .Select(g => new Fleet
                {
                    ID = g.Key.ID,
                    Name = g.Key.Caption,
                    Sprite = g.Select(b => b.Sprite).FirstOrDefault(),
                    Color = g.Key.Color,
                    Ships = g.Select(b => new Ship
                    {
                        ID = b.ID,
                        Angle = b.Angle,
                        Momentum = b.Momentum,
                        Position = b.Position,
                        Size = b.Size
                    }).ToList()
                })
                .ToList();

            foreach (var fleet in AllVisibleFleets)
                fleet.PendingDestruction = true;
            foreach (var fleet in newFleets)
            {
                var existing = AllVisibleFleets.FirstOrDefault(f => f.ID == fleet.ID);
                if (existing != null)
                {
                    existing.Name = fleet.Name;
                    existing.Sprite = fleet.Sprite;
                    existing.Color = fleet.Color;
                    existing.PendingDestruction = false;

                    foreach (var ship in existing.Ships)
                        ship.PendingDestruction = true;

                    foreach (var ship in fleet.Ships)
                    {
                        var existingShip = existing.Ships.FirstOrDefault(s => s.ID == ship.ID);
                        if (existingShip != null)
                        {
                            existingShip.Position = ship.Position;
                            existingShip.Momentum = ship.Momentum;
                            existingShip.Size = ship.Size;
                            existingShip.Angle = ship.Angle;
                            ship.PendingDestruction = false;
                        }
                        else
                            existing.Ships.Add(ship);

                        existing.Ships = existing.Ships.Where(s => !s.PendingDestruction).ToList();
                    }
                }
                else
                    AllVisibleFleets.Add(fleet);
            }
            AllVisibleFleets = AllVisibleFleets.Where(f => !f.PendingDestruction).ToList();

            MyFleet = AllVisibleFleets.FirstOrDefault(f => f.ID == Robot.FleetID);
        }

        public IEnumerable<Fleet> Others
        {
            get => MyFleet != null
                ? AllVisibleFleets.Except(new[] { MyFleet })
                : AllVisibleFleets;
        }
    }
}
