namespace Game.Engine.Networking
{
    using Game.Engine.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class BodyCache
    {
        private readonly Dictionary<long, BucketBody> Bodies = new Dictionary<long, BucketBody>();
        private readonly Dictionary<long, BucketGroup> Groups = new Dictionary<long, BucketGroup>();

        public void Update(IEnumerable<Body> bodies, uint time)
        {
            
            // update cache items and flag missing ones as stale
            UpdateLocalBodies(bodies);

            // project the current bodies and calculate errors
            foreach (var bucket in Bodies.Values)
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

        private void UpdateLocalGroups(IEnumerable<Group> groups)
        {
            foreach (var bucket in Groups.Values)
                bucket.Stale = true;

            foreach (var obj in groups)
            {
                BucketGroup bucket = null;

                if (Groups.ContainsKey(obj.ID))
                {
                    Groups[obj.ID].Stale = false;
                }
                else
                {
                    bucket = new BucketGroup
                    {
                        GroupUpdated = obj,
                        Stale = false
                    };
                    Groups.Add(obj.ID, bucket);
                }
            }
        }

        private void UpdateLocalBodies(IEnumerable<Body> bodies)
        {
            foreach (var bucket in Groups.Values)
                bucket.Stale = true;

            foreach (var bucket in Bodies.Values)
                bucket.Stale = true;

            foreach (var obj in bodies)
            {
                if (Bodies.ContainsKey(obj.ID))
                {
                    Bodies[obj.ID].Stale = false;
                    Bodies[obj.ID].BodyUpdated = obj;
                }
                else
                {
                    var bucket = new BucketBody
                    {
                        BodyUpdated = obj,
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
                Bodies.Remove(b.BodyUpdated.ID);

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
            public Body BodyUpdated { get; set; }
            public Body BodyClient { get; set; }

            public float Error { get; set; }
            public bool Stale { get; set; }

            private const int DISTANCE_THRESHOLD = 2;
            private const float WEIGHT_DISTANCE = 1;
            private const float WEIGHT_ANGLE = 10;
            private const float WEIGHT_SPRITE = 1;
            private const float WEIGHT_SIZE = 1;
            private const float WEIGHT_MODE = 1;
            private const float WEIGHT_MISSING = float.MaxValue;

            public void Project(uint time)
            {
                if (BodyClient != null)
                {
                    if (BodyClient.DefinitionTime == BodyUpdated.DefinitionTime)
                        Error = 0;
                    else
                    {
                        BodyClient.Project(time);
                        var distance = Vector2.Distance(BodyClient.Position, BodyUpdated.Position);
                        Error =
                            distance > DISTANCE_THRESHOLD
                                ? WEIGHT_DISTANCE * distance
                                : 0
                            + WEIGHT_ANGLE * Math.Abs(BodyClient.Angle - BodyUpdated.Angle)
                            + WEIGHT_SIZE * Math.Abs(BodyClient.Size - BodyUpdated.Size)
                            + WEIGHT_MODE * Math.Abs(BodyClient.Mode - BodyUpdated.Mode)
                            + WEIGHT_SPRITE * (BodyClient.Sprite != BodyUpdated.Sprite ? 1 : 0);
                    }
                }
                else
                    Error = WEIGHT_MISSING;
            }
        }
    }
}
