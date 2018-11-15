namespace Game.Engine.Core
{
    public class Group
    {
        public uint ID { get; set; }

        public bool Exists { get; set; }
        public bool IsDirty { get; set; } = true;

        private byte _groupType { get; set; }
        public virtual byte GroupType
        {
            get
            {
                return _groupType;
            }
            set
            {
                IsDirty = IsDirty || _groupType != value;
                _groupType = value;
            }
        }

        private string _caption { get; set; }
        public virtual string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                IsDirty = IsDirty || _caption != value;
                _caption = value;
            }
        }
    }
}
