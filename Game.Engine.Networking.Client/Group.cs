namespace Game.API.Client
{
    public class Group
    {
        public uint ID { get; set; }
        public byte Type { get; set; }
        public string Caption { get; set; }
        public uint ZIndex { get; set; }
        public uint Owner { get; set; }
    }
}
