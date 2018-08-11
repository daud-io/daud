namespace Game.Engine.Networking
{
    using Game.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class BodyCache
    {
        private readonly Dictionary<long, Bucket> Buckets = new Dictionary<long, Bucket>();

        public void Update(IEnumerable<GameObject> Objects, long time)
        {
            CreateMissingBuckets(Objects);

            foreach (var bucket in Buckets.Values)
            {
                bucket.Project(time);
            }

            var staleBuckets = Buckets.Values
                .Where(b => b.Error > 0)
                .OrderByDescending(b => b.Error);
        }

        private void CreateMissingBuckets(IEnumerable<GameObject> Objects)
        {
            foreach (var go in Objects)
            {
                Bucket bucket = null;

                if (Buckets.ContainsKey(go.ID))
                    bucket = Buckets[go.ID];
                else
                {
                    bucket = new Bucket
                    {
                        BodyUpdated = new ProjectedBody
                        {
                            ID = go.ID
                        }
                    };
                    Buckets.Add(go.ID, bucket);
                }
            }
        }

        class Bucket
        {
            public ProjectedBody BodyUpdated { get; set; }
            public ProjectedBody BodyClient { get; set; }

            public float Error { get; set; }

            private const float WEIGHT_DISTANCE = 1;
            private const float WEIGHT_ANGLE = 1;
            private const float WEIGHT_CAPTION = 1;
            private const float WEIGHT_SPRITE = 1;
            private const float WEIGHT_MISSING = float.MaxValue;

            public void Project(long time)
            {
                BodyUpdated.Project(time);
                if (BodyClient != null)
                {
                    BodyClient.Project(time);
                    Error =
                        WEIGHT_DISTANCE * Vector2.Distance(BodyClient.Position, BodyUpdated.Position)
                        + WEIGHT_ANGLE * Math.Abs(BodyClient.Angle - BodyUpdated.Angle)
                        + WEIGHT_CAPTION * (BodyClient.Caption != BodyUpdated.Caption ? 1 : 0)
                        + WEIGHT_SPRITE * (BodyClient.Sprite != BodyUpdated.Sprite ? 1 : 0);
                }
                else
                    Error = WEIGHT_MISSING;
            }
        }
    }
}
