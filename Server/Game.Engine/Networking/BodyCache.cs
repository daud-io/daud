namespace Game.Engine.Networking
{
    using DaudNet;
    using Game.API.Common;
    using Game.Engine.Core;
    using System;
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

            if (!Bodies.TryGetValue(worldBody.ID, out BucketBody bucket))
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
                if (!Groups.TryGetValue(worldBody.Group.ID, out BucketGroup group))
                {
                    group = new BucketGroup();
                    Groups.Add(worldBody.Group.ID, group);
                    group.Stale = true;
                }

                if (group.Stale)
                {
                    group.Stale = false;
                    group.ReadGroup(worldBody.Group, time);
                }
            }
        }

        public void CollectStaleBuckets()
        {
            var original = this.StaleBodyCount;
            foreach (var b in Bodies.Values)
                if (b.Stale && this.StaleBodyCount < this.StaleBodies.Length)
                    this.StaleBodies[this.StaleBodyCount++] = b.NetBody.id;

            for (int i=original; i<this.StaleBodyCount; i++)
                Bodies.Remove(this.StaleBodies[i]);
        }

        public void CollectStaleGroups()
        {
            var original = this.StaleGroupCount;

            foreach (var b in Groups.Values)
                if (b.Stale && this.StaleGroupCount < this.StaleGroups.Length)
                    this.StaleGroups[this.StaleGroupCount++] = b.NetGroup.group;

            for (int i=original; i<this.StaleGroupCount; i++)
                Groups.Remove(this.StaleGroups[i]);
        }

        public class BucketGroup
        {
            public bool Stale { get; set; } = true;
            public float Error { get; set; }
            public NetGroup NetGroup { get; internal set; }

            public BucketGroup()
            {
                this.NetGroup = new();
            }

            public void ReadGroup(Group group, uint time)
            {
                this.Error = 1;
                this.NetGroup.group = group.ID;
                this.NetGroup.type = (byte)group.GroupType;
                this.NetGroup.caption = group.Caption;
                this.NetGroup.zindex = group.ZIndex;
                this.NetGroup.owner = group.OwnerID;
                this.NetGroup.color = group.Color;
                this.NetGroup.customdata = group.CustomData;
            }
        }

        public class BucketBody
        {
            public uint ClientUpdatedTime;

            public float Error = 0;
            public bool Stale = false;

            public DaudNet.NetBody NetBody;
            public BucketBody()
            {
                this.NetBody = new DaudNet.NetBody
                {
                    originalposition = new DaudNet.Vec2(),
                    velocity = new DaudNet.Vec2()
                };
            }

            public void UpdateSent(uint time)
            {
                ClientUpdatedTime = time;
                Error = 0;
            }

            public void ReadBody(WorldBody body, uint time)
            {
                const float VELOCITY_SCALE_FACTOR = 5000;

                this.NetBody.id = body.ID;
                this.NetBody.definitiontime = time;
                this.NetBody.originalposition.x = (short)body.Position.X;
                this.NetBody.originalposition.y = (short)body.Position.Y;
                this.NetBody.velocity.x = (short)(body.LinearVelocity.X * VELOCITY_SCALE_FACTOR);
                this.NetBody.velocity.y = (short)(body.LinearVelocity.Y * VELOCITY_SCALE_FACTOR);
                this.NetBody.originalangle = (sbyte)(body.Angle / MathF.PI * 127);
                this.NetBody.angularvelocity = (sbyte)(body.AngularVelocity * 10000);
                this.NetBody.size = (byte)(body.Size / 5);
                this.NetBody.sprite = (ushort)body.Sprite;
                this.NetBody.mode = body.Mode;
                this.NetBody.group = body.Group?.ID ?? 0;

                Error = time - ClientUpdatedTime;
                if (Error == 0)
                    Error = 1;
            }
        }
    }
}
