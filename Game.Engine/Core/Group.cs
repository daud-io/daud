namespace Game.Engine.Core
{
    using Game.API.Common;

    public class Group
    {
        public uint ID { get; set; }

        public bool Exists { get; set; }
        public bool IsDirty { get; set; } = true;

        private GroupTypes _groupType;
        public virtual GroupTypes GroupType
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

        private string _caption;
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

        private string _color;
        public virtual string Color
        {
            get
            {
                return _color;
            }
            set
            {
                IsDirty = IsDirty || _color != value;
                _color = value;
            }
        }

        private uint _zIndex;
        public virtual uint ZIndex
        {
            get
            {
                return _zIndex;
            }
            set
            {
                IsDirty = IsDirty || _zIndex != value;
                _zIndex = value;
            }
        }

        private uint _ownerID;
        public virtual uint OwnerID
        {
            get
            {
                return _ownerID;
            }
            set
            {
                IsDirty = IsDirty || _ownerID != value;
                _ownerID = value;
            }
        }

        public Group Clone()
        {
            return this.MemberwiseClone() as Group;
        }
    }
}
