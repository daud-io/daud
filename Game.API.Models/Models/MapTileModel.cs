namespace Game.API.Common.Models
{
    using System.Numerics;

    public class MapTileModel
    {
        public Vector2 Position { get; set; }
        public int TileGridID { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
    }
}
