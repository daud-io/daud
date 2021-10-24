namespace Game.API.Common.Models
{
    public class Announcement
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public int PointsDelta { get; set; }
        public string ExtraData { get; set; }
    }
}
