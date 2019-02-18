namespace Game.Engine.Core.Maps
{
    using Game.API.Common;
    using Game.API.Common.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class MapActor : IActor
    {
        private World World = null;
        private WorldMap Map { get; set; } = null;

        private MapModel NewMapModel = null;

        public void Think()
        {
            if (Map != null)
            {
                foreach (var ship in AllShips().ToList())
                {
                    var tile = Map.TileFromPosition(ship.Position);
                    if (tile != null)
                        tile.InteractWithShip(ship);
                }
            }
        }

        private IEnumerable<Ship> AllShips()
        {
            return Player.GetWorldPlayers(World)
                .Where(p => p.IsAlive)
                .Where(p => p.Fleet != null)
                .Where(p => p.Fleet.Ships.Any())
                .SelectMany(p => p.Fleet.Ships);
        }

        void IActor.CreateDestroy()
        {
            if (World.Hook.MapEnabled && Map == null)
            {
                Map = new WorldMap();
                Map.Init(World);
            }

            if (!World.Hook.MapEnabled && Map != null)
            {
                Map.PendingDestruction = true;
                Map = null;
            }

            if (NewMapModel != null && Map != null)
            {
                LoadTiles(NewMapModel);
            }
        }

        void IActor.Destroy()
        {
            World.Actors.Remove(this);
            if (Map != null)
                Map.PendingDestruction = true;

            Map = null;
        }

        void IActor.Init(World world)
        {
            World = world;
            World.Actors.Add(this);
        }

        public void SetMap(MapModel map)
        {
            this.NewMapModel = map;
        }

        private void LoadTiles(MapModel mapModel)
        {
            if (Map != null)
            {
                Map.DestroyTiles();
                Map.Size = mapModel.TileSize;
                Map.Columns = mapModel.Columns;
                Map.Rows = mapModel.Rows;

                Map.Offset = new Vector2(-mapModel.TileSize * mapModel.Columns / 2f, -mapModel.TileSize * mapModel.Rows / 2f);
                var halfTile = new Vector2(mapModel.TileSize / 2, mapModel.TileSize / 2);
                if (mapModel.Tiles != null)
                {
                    foreach (var tile in mapModel.Tiles)
                    {
                        if (tile.TileGridID != -1)
                        {
                            TileBase tileObject;
                            if (tile.Type == "turret")
                                tileObject = new TileTurret();
                            else
                                tileObject = new TileBase();

                            tileObject.Sprite = (Sprites)(ushort)(1000 + tile.TileGridID);
                            tileObject.Position =
                                new Vector2(tile.Column * mapModel.TileSize, tile.Row * mapModel.TileSize) // top left
                                + halfTile // center
                                + Map.Offset; // offset map
                            tileObject.Size = mapModel.TileSize / 2;
                            tileObject.Drag = tile.Drag;

                            Map.AddTile(tileObject);
                        }
                        else
                            Map.AddTile(null);
                    }
                }

                NewMapModel = null;
            }
        }
    }
}
