namespace Game.API.Client
{
    using Game.API.Common;

    public class Group
    {
        public uint ID { get; set; }
        public GroupTypes Type { get; set; }
        public string Caption { get; set; }
        public uint ZIndex { get; set; }
        public uint Owner { get; set; }
        public string Color { get; set; }
    }
}
