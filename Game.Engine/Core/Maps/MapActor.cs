namespace Game.Engine.Core.Maps
{
    using Game.API.Common;
    using Game.API.Common.Models;
    using System.Collections.Generic;

    public class MapActor : IActor
    {
        private World World = null;
        private WorldMap Map { get; set; } = null;

        private IEnumerable<MapTileModel> NewTileSet = null;

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

            if (NewTileSet != null && Map != null)
            {
                LoadTiles(NewTileSet);
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

        public void SetTiles(IEnumerable<MapTileModel> tiles)
        {
            this.NewTileSet = tiles;
        }

        private void LoadTiles(IEnumerable<MapTileModel> tiles)
        {
            if (Map != null)
            {
                Map.DestroyTiles();
                foreach (var tile in tiles)
                    Map.AddTile(new Tile
                    {
                        Sprite = (Sprites)(ushort)(1000 + tile.TileGridID),
                        Position = tile.Position,
                        Size = tile.Size,
                        IsDeadly = (tile.Type == "deadly"),
                        IsObstacle = (tile.Type == "obstacle"),
                        IsBouncy = (tile.Type == "bouncy"),
                        IsTurret = (tile.Type == "turret"),
                        Drag = tile.Drag
                    });

                NewTileSet = null;
            }
        }

        public void Think()
        {
        }
    }
}
