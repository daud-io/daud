namespace Game.API.Common.Models
{
    public class MapTileModel
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int TileGridID { get; set; }
        public string Type { get; set; }
        public float Drag { get; set; } = 0;
    }
}
