namespace Game.Engine.Core.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class WorldMap : ActorGroup
    {
        private List<TileBase> Tiles = new List<TileBase>();
        public Group WeaponGroup { get; set; } = new Group();
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Size { get; set; }
        public Vector2 Offset { get; set; }

        public override void Init(World world)
        {
            base.Init(world);
            this.GroupType = API.Common.GroupTypes.Map;
            this.ZIndex = 255;
            this.WeaponGroup.GroupType = API.Common.GroupTypes.VolleyBullet;
            this.WeaponGroup.OwnerID = this.ID;
        }

        public override void CreateDestroy()
        {
            base.CreateDestroy();
        }

        public void DestroyTiles()
        {
            foreach (var tile in Tiles.Where(t => t != null))
                tile.PendingDestruction = true;

            Tiles.Clear();
        }

        public void AddTile(TileBase tile)
        {
            if (tile != null)
            {
                tile.Init(World);
                tile.WorldMap = this;
                tile.Group = this;
            }
            Tiles.Add(tile);
        }

        public TileBase TileFromPosition(Vector2 position)
        {
            if (Size == 0 || !Tiles.Any())
                return null;

            var gridCoordinates = (position - Offset);
            gridCoordinates /= Size;

            var index = ((int)gridCoordinates.Y * Columns) + (int)gridCoordinates.X;

            return (index >= 0 && index < Tiles.Count)
                ? Tiles[index]
                : null;
        }
    }
}
