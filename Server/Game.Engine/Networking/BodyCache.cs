namespace Game.Engine.Networking
{
    using Game.API.Common;
    using Game.Engine.Core;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class BodyCache
    {
        private readonly Dictionary<long, BucketBody> Bodies = new Dictionary<long, BucketBody>();
        private readonly Dictionary<long, BucketGroup> Groups = new Dictionary<long, BucketGroup>();

        public uint[] StaleGroups = new uint[1024];
        public int StaleGroupCount;
        public uint[] StaleBodies = new uint[1024];
        public int StaleBodyCount = 0;
        
        public void PreUpdate()
        {
            foreach (var bucket in Groups.Values)
                bucket.Stale = true;

            foreach (var bucket in Bodies.Values)
                bucket.Stale = true;
        }

        public IEnumerable<BucketBody> BodiesByError()
        {
            return Bodies.Values.Where(b => !b.Stale && b.Error > 0);
        }

        public IEnumerable<BucketGroup> GroupsByError()
        {
            return Groups.Values.Where(b => !b.Stale && b.Error > 0);
        }

        public void UpdateCachedBody(WorldBody worldBody, uint time)
        {
            if (worldBody == null || !worldBody.Exists)
                return;

            BucketBody bucket;
            if (!Bodies.TryGetValue(worldBody.ID, out bucket))
            {
                bucket = new BucketBody()
                {
                    Stale = false
                };
                Bodies.Add(worldBody.ID, bucket);
            }

            bucket.Stale = false;
            bucket.ReadBody(worldBody, time);
            
            if (worldBody.Group != null)
            {
                BucketGroup group;
                if (!Groups.TryGetValue(worldBody.Group.ID, out group))
                {
                    group = new BucketGroup();
                    group.ID = worldBody.Group.ID;
                    Groups.Add(group.ID, group);
                }

                group.Stale = false;
                group.ReadGroup(worldBody.Group, time);
            }
        }

        public void CollectStaleBuckets()
        {
            var original = this.StaleBodyCount;
            foreach (var b in Bodies.Values)
                if (b.Stale && this.StaleBodyCount < this.StaleBodies.Length)
                    this.StaleBodies[this.StaleBodyCount++] = b.ID;

            for (int i=original; i<this.StaleBodyCount; i++)
                Bodies.Remove(this.StaleBodies[i]);
        }

        public void CollectStaleGroups()
        {
            var original = this.StaleGroupCount;

            foreach (var b in Groups.Values)
                if (b.Stale && this.StaleGroupCount < this.StaleGroups.Length)
                    this.StaleGroups[this.StaleGroupCount++] = b.ID;

            for (int i=original; i<this.StaleGroupCount; i++)
                Groups.Remove(this.StaleGroups[i]);
        }

        public class BucketGroup
        {
            public uint ID;
            public string CustomData;
            public string Color;
            public string Caption;
            public GroupTypes GroupType;
            public uint OwnerID;
            public uint ZIndex;



            public bool Stale { get; set; }
            public float Error { get; set; }

            public void ReadGroup(Group group, uint time)
            {
                if (this.CustomData != group.CustomData
                    || this.Color != group.Color
                    || this.GroupType != group.GroupType
                    || this.Caption != group.Caption
                    || this.OwnerID != group.OwnerID
                    || this.ZIndex != group.ZIndex
                    )
                {
                    this.Error = 1;
                    this.CustomData = group.CustomData;
                    this.Color = group.Color;
                    this.Caption = group.Caption;
                    this.GroupType = group.GroupType;
                    this.OwnerID = group.OwnerID;
                    this.ZIndex = group.ZIndex;
                }
                else
                    this.Error = 0;
            }
        }

        public class BucketBody
        {
            public uint DefinitionTime;
            public Vector2 Position;
            public Vector2 LinearVelocity;
            public float AngularVelocity;
            public float Angle;
            public int Size = 0;
            public byte Mode = 0;
            public Sprites Sprite;

            public uint GroupID = 0;
            public uint ID = 0;

            public uint ClientUpdatedTime;

            public float Error = 0;
            public bool Stale = false;

            public void UpdateSent(uint time)
            {
                ClientUpdatedTime = time;
                Error = 0;
            }

            public void ReadBody(WorldBody body, uint time)
            {
                DefinitionTime = time;
                Position = body.Position;
                LinearVelocity = body.LinearVelocity;
                AngularVelocity = body.AngularVelocity;
                Angle = body.Angle;
                Size = body.Size;
                Mode = body.Mode;
                Sprite = body.Sprite;
                GroupID = body.Group?.ID ?? 0;
                ID = body.ID;

                Error = time - ClientUpdatedTime;
                if (Error == 0)
                    Error = 1;
            }
        }
    }
}
