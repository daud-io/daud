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

        public void Update(IEnumerable<WorldBody> bodies, uint time)
        {
            // update cache items and flag missing ones as stale
            UpdateLocalBodies(bodies);

            // project the current bodies and calculate errors
            foreach (var bucket in Bodies.Values)
                if(!bucket.Stale)
                    bucket.Project(time);

            foreach (var bucket in Groups.Values)
                bucket.CalculateError();
        }

        public IEnumerable<BucketBody> BodiesByError()
        {
            // find the bodies with the largest error
            return Bodies.Values
                .Where(b => !b.Stale)
                .Where(b => b.Error > 0)
                .OrderByDescending(b => b.Error);
        }

        public IEnumerable<BucketGroup> GroupsByError()
        {
            // find the bodies with the largest error
            return Groups.Values
                .Where(b => !b.Stale)
                .Where(b => b.Error > 0)
                .OrderByDescending(b => b.Error);
        }

        private void UpdateLocalBodies(IEnumerable<WorldBody> bodies)
        {
            foreach (var bucket in Groups.Values)
                bucket.Stale = true;

            foreach (var bucket in Bodies.Values)
                bucket.Stale = true;

            foreach (var obj in bodies)
            {
                if (obj == null || !obj.Exists)
                    continue;

                BucketBody bucket;
                if (!Bodies.TryGetValue(obj.ID, out bucket))
                {
                    bucket = new BucketBody()
                    {
                        Stale = false
                    };
                    Bodies.Add(obj.ID, bucket);
                }

                bucket.Stale = false;
                bucket.ReadBody(obj);

                if (obj.Group != null)
                    if (Groups.ContainsKey(obj.Group.ID))
                        Groups[obj.Group.ID].Stale = false;
                    else
                    {
                        var bucketGroup = new BucketGroup
                        {
                            GroupUpdated = obj.Group,
                            Stale = false
                        };
                        Groups.Add(obj.Group.ID, bucketGroup);
                    }
            }
        }

        public IEnumerable<BucketBody> CollectStaleBuckets()
        {
            var stale = Bodies.Values.Where(b => b.Stale).ToList();
            foreach (var b in stale)
                Bodies.Remove(b.ID);

            return stale;
        }

        public IEnumerable<BucketGroup> CollectStaleGroups()
        {
            var stale = Groups.Values.Where(b => b.Stale).ToList();
            foreach (var b in stale)
                Groups.Remove(b.GroupUpdated.ID);

            return stale;
        }

        public class BucketGroup
        {
            public Group GroupUpdated { get; set; }
            public Group GroupClient { get; set; }

            public bool Stale { get; set; }
            public float Error { get; set; }

            public void CalculateError()
            {
                if (GroupClient == null
                    || GroupClient.CustomData != GroupUpdated.CustomData
                    || GroupClient.Color != GroupUpdated.Color
                    || GroupClient.GroupType != GroupClient.GroupType
                    || GroupClient.Caption != GroupClient.Caption
                    )

                    Error = 1;
                else
                    Error = 0;
            }
        }

        public class BucketBody
        {
            //public WorldBody Body { get; set; }

            public Vector2 Position;
            public Vector2 LinearVelocity;
            public float AngularVelocity;
            public float Angle;
            public int Size = 0;
            public byte Mode = 0;
            public Sprites Sprite;

            public uint GroupID = 0;
            public uint ID = 0;

            public uint ClientUpdatedTime { get; set;}

            public float Error { get; set; }
            public bool Stale { get; set; }

            public void UpdateSent(uint time)
            {
                ClientUpdatedTime = time;
            }

            public void ReadBody(WorldBody body)
            {
                Position = body.Position;
                LinearVelocity = body.LinearVelocity;
                AngularVelocity = body.AngularVelocity;
                Angle = body.Angle;
                Size = body.Size;
                Mode = body.Mode;
                Sprite = body.Sprite;
                GroupID = body.Group?.ID ?? 0;
                ID = body.ID;
            }

            public void Project(uint time)
            {
                Error = time - ClientUpdatedTime;
                if (Error == 0)
                    Error = 1;
            }
        }
    }
}
