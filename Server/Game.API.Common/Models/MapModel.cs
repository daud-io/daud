namespace Game.API.Common.Models
{
    using System.Collections.Generic;
    using System.Numerics;

    public class MapModel
    {
        public Vector2 Position { get; set; }
        public int TileSize { get; set; }
        public string Type { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public List<MapTileModel> Tiles { get; set; }
    }
}
