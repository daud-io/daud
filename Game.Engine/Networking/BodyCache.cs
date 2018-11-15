namespace Game.Engine.Networking
{
    using Game.Engine.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class BodyCache
    {
        private readonly Dictionary<long, Bucket> Buckets = new Dictionary<long, Bucket>();
        private readonly Dictionary<long, object> Groups = new Dictionary<long, object>();

        public IEnumerable<Bucket> Update(IEnumerable<Body> bodies, uint time, Vector2 windowTopLeft, Vector2 windowBottomRight)
        {
            // this should be some more efficient query r-trees or something
            var filtered = bodies.Where(b =>
                b.Position.X >= windowTopLeft.X
                && b.Position.Y >= windowTopLeft.Y
                && b.Position.X <= windowBottomRight.X
                && b.Position.Y <= windowBottomRight.Y
            ).ToList();

            // update cache items and flag missing ones as stale
            UpdateLocalBodies(filtered);

            // project the current bodies and calculate errors
            foreach (var bucket in Buckets.Values)
                bucket.Project(time);

            // find the bodies with the largest error
            return Buckets.Values
                .Where(b => !b.Stale)
                .Where(b => b.Error > 0)
                .OrderByDescending(b => b.Error);
        }

        private void UpdateLocalBodies(IEnumerable<Body> bodies)
        {
            foreach (var bucket in Buckets.Values)
                bucket.Stale = true;

            foreach (var obj in bodies)
            {
                Bucket bucket = null;

                if (Buckets.ContainsKey(obj.ID))
                {
                    Buckets[obj.ID].Stale = false;
                    Buckets[obj.ID].BodyUpdated = obj;
                }
                else
                {
                    bucket = new Bucket
                    {
                        BodyUpdated = obj,
                        Stale = false
                    };
                    Buckets.Add(obj.ID, bucket);
                }
            }
        }

        public IEnumerable<Bucket> CollectStaleBuckets()
        {
            var stale = Buckets.Values.Where(b => b.Stale).ToList();
            foreach (var b in stale)
                Buckets.Remove(b.BodyUpdated.ID);

            return stale;
        }

        public class Bucket
        {
            public Body BodyUpdated { get; set; }
            public Body BodyClient { get; set; }

            public float Error { get; set; }
            public bool Stale { get; set; }

            private const int DISTANCE_THRESHOLD = 2;
            private const float WEIGHT_DISTANCE = 1;
            private const float WEIGHT_ANGLE = 10;
            private const float WEIGHT_SPRITE = 1;
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
                            + WEIGHT_SPRITE * (BodyClient.Sprite != BodyUpdated.Sprite ? 1 : 0);
                    }
                }
                else
                    Error = WEIGHT_MISSING;
            }
        }
    }
}
