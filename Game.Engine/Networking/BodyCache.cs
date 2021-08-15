namespace Game.Engine.Networking
{
    using Game.API.Common;
    using Game.Engine.Core;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
                if (!obj.Exists)
                    continue;

                if (Bodies.ContainsKey(obj.ID))
                {
                    Bodies[obj.ID].Stale = false;
                    Debug.Assert(Bodies[obj.ID].Body == obj);
                }
                else
                {
                    var bucket = new BucketBody
                    {
                        Body = obj,
                        Stale = false
                    };
                    Bodies.Add(obj.ID, bucket);
                }

                if (obj.Group != null)
                    if (Groups.ContainsKey(obj.Group.ID))
                        Groups[obj.Group.ID].Stale = false;
                    else
                    {
                        var bucket = new BucketGroup
                        {
                            GroupUpdated = obj.Group,
                            Stale = false
                        };
                        Groups.Add(obj.Group.ID, bucket);
                    }
            }
        }

        public IEnumerable<BucketBody> CollectStaleBuckets()
        {
            var stale = Bodies.Values.Where(b => b.Stale).ToList();
            foreach (var b in stale)
                Bodies.Remove(b.Body.ID);

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
            public WorldBody Body { get; set; }

            public Vector2 Position { get; set;}
            public Vector2 LinearVelocity { get; set;}
            public float AngularVelocity { get; set;}
            public float Angle { get; set;}
            public int Size = 0;
            public byte Mode = 0;
            public Sprites Sprite;

            public uint ClientUpdatedTime { get; set;}

            public float Error { get; set; }
            public bool Stale { get; set; }

            private const int DISTANCE_THRESHOLD = 2;
            private const float WEIGHT_DISTANCE = 1;
            private const float WEIGHT_ANGLE = 10;
            private const float WEIGHT_SPRITE = 1;
            private const float WEIGHT_SIZE = 1;
            private const float WEIGHT_MODE = 1;
            private const float WEIGHT_MISSING = float.MaxValue;

            public void UpdateSent(uint time)
            {
                ClientUpdatedTime = time;
                Position = Body.Position;
                LinearVelocity = Body.LinearVelocity;
                AngularVelocity = Body.AngularVelocity;
                Angle = Body.Angle;
                Size = Body.Size;
                Mode = Body.Mode;
                Sprite = Body.Sprite;
            }

            public void Project(uint time)
            {
                if (ClientUpdatedTime != 0)
                {
                    var timeDelta = (time - this.ClientUpdatedTime);

                    var position = Vector2.Add(Position, Vector2.Multiply(LinearVelocity, timeDelta));
                    var angle = Angle + timeDelta * AngularVelocity;

                    var distance = Vector2.Distance(position, Body.Position);
                    Error =
                        distance > DISTANCE_THRESHOLD
                            ? WEIGHT_DISTANCE * distance
                            : 0
                        + WEIGHT_ANGLE * Math.Abs(angle - Body.Angle)
                        + WEIGHT_SIZE * Math.Abs(Size - Body.Size)
                        + WEIGHT_MODE * Math.Abs(Mode - Body.Mode)
                        + WEIGHT_SPRITE * (Sprite != Body.Sprite ? 1 : 0);
                }
                else
                    Error = WEIGHT_MISSING;
            }
        }
    }
}
