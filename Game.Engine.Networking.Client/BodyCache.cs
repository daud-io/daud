namespace Game.API.Client
{
    using System.Collections.Generic;

    public class BodyCache
    {
        private readonly Dictionary<uint, ProjectedBody> _bodies = new Dictionary<uint, ProjectedBody>();
        public void Update(IEnumerable<ProjectedBody> updates, IEnumerable<uint> deletes)
        {
            foreach (var id in deletes)
                _bodies.Remove(id);

            foreach (var update in updates)
            {
                if (!_bodies.ContainsKey(update.ID))
                    _bodies.Add(update.ID, update);
                else
                {
                    var existing = _bodies[update.ID];
                    existing.Size = update.Size;
                    existing.Sprite = update.Sprite;
                    existing.Caption = update.Caption;
                    existing.Color = update.Color;
                    existing.OriginalAngle = update.OriginalAngle;
                    existing.AngularVelocity = update.AngularVelocity;
                }
            }
        }

        public IEnumerable<ProjectedBody> Bodies
        {
            get
            {
                return this._bodies.Values;
            }
        }
    }
}
