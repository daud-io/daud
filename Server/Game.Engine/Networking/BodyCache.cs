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
            return Bodies.Values
                .Where(b => !b.Stale && b.Error > 0)
                .OrderByDescending(b => b.Error);
        }

        public IEnumerable<BucketGroup> GroupsByError()
        {
            return Groups.Values
                .Where(b => !b.Stale && b.Error > 0)
                .OrderByDescending(b => b.Error);
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
                this.NetGroup.group = group.ID;
                this.NetGroup.type = (byte)group.GroupType;

                if (this.NetGroup.caption != group.Caption)
                {
                    this.NetGroup.caption = group.Caption;
                    Error++;
                }

                if (this.NetGroup.zindex != group.ZIndex)
                {
                    this.NetGroup.zindex = group.ZIndex;
                    Error++;
                }

                if (this.NetGroup.owner != group.OwnerID)
                {
                    this.NetGroup.owner = group.OwnerID;
                    Error++;
                }
                if (this.NetGroup.color != group.Color)
                {
                    this.NetGroup.color = group.Color;
                    Error++;
                }
                if (this.NetGroup.customdata != group.CustomData)
                {
                    this.NetGroup.customdata = group.CustomData;
                    Error++;
                }
            }
        }

        public class BucketBody
        {
            public uint ClientUpdatedTime;

            public float Error = 0;
            public bool Stale = false;

            private uint _Time;
            private Vector2 _Position;
            private Vector2 _LinearVelocity;
            private float _Angle;
            private float _AngularVelocity;
            private int _Size;

            private Sprites _Sprite;
            private byte _Mode;
            private uint _GroupID;

            public DaudNet.NetBody NetBody;
            public BucketBody()
            {
                this.NetBody = new DaudNet.NetBody
                {
                    originalposition = new DaudNet.Vec2(),
                    velocity = new DaudNet.Vec2()
                };
            }

            public void DoUpdate()
            {
                const float VELOCITY_SCALE_FACTOR = 5000;

                this.NetBody.definitiontime = _Time;
                this.NetBody.originalposition.x = (short)_Position.X;
                this.NetBody.originalposition.y = (short)_Position.Y;
                this.NetBody.velocity.x = (short)(_LinearVelocity.X * VELOCITY_SCALE_FACTOR);
                this.NetBody.velocity.y = (short)(_LinearVelocity.Y * VELOCITY_SCALE_FACTOR);
                this.NetBody.originalangle = (sbyte)(_Angle / MathF.PI * 127);
                this.NetBody.angularvelocity = (sbyte)(_AngularVelocity * 10000);
                this.NetBody.size = (byte)(_Size / 5);
                this.NetBody.sprite = (ushort)_Sprite;
                this.NetBody.mode = _Mode;
                this.NetBody.group = _GroupID;

                ClientUpdatedTime = _Time;
                Error = 0;
            }

            public void ReadBody(WorldBody body, uint time)
            {
                float newError = 0;
                const float minimumDistanceSquared = 10*10;
                const float minimumVelocityDeltaSquared = 0.01f;
                const float minimumAngleDelta = MathF.PI/180f;
                
                _Time = time;
                this.NetBody.id = body.ID;

                if (_Position != body.Position)
                {
                    var dist = Vector2.DistanceSquared(_Position, body.Position);
                    if (dist > minimumDistanceSquared)
                        newError += 1;

                    _Position = body.Position;
                }

                if(_LinearVelocity != body.LinearVelocity)
                {
                    var dist = Vector2.DistanceSquared(_LinearVelocity, body.LinearVelocity);
                    if (dist > minimumVelocityDeltaSquared)
                        newError += 1;
                    _LinearVelocity = body.LinearVelocity;

                }

                if (_Angle != body.Angle)
                {
                    var dist = MathF.Abs(_Angle-body.Angle) % MathF.PI * 2;
                    if (dist > minimumAngleDelta)
                        newError++;

                    _Angle = body.Angle;
                }

                if(_AngularVelocity != body.AngularVelocity)
                {
                    _AngularVelocity = body.AngularVelocity;
                    newError++;
                }

                if (_Size != body.Size)
                {
                    _Size = body.Size;
                    newError++;
                }

                if (_Sprite != body.Sprite)
                {
                    _Sprite = body.Sprite;
                    newError++;
                }

                if (_GroupID != (body.Group?.ID ?? 0))
                {
                    _GroupID = body.Group?.ID ?? 0;
                    newError++;
                }

                Error += newError;
            }
        }
    }
}
