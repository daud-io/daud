namespace Game.API.Client
{
    using System.Collections.Generic;

    public class BodyCache
    {
        private readonly Dictionary<int, ProjectedBody> _bodies = new Dictionary<int, ProjectedBody>();
        public void Update(IEnumerable<ProjectedBody> updates, IEnumerable<int> deletes)
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
                    if (update.Size != -1)
                        existing.Size = update.Size;
                    if (update.Sprite != null)
                        existing.Sprite = update.Sprite;
                    if (update.Caption != null)
                        existing.Caption = update.Caption;
                    if (update.Color != null)
                        existing.Color = update.Color;

                    if (update.OriginalAngle != -999)
                        existing.OriginalAngle = update.OriginalAngle;
                    if (update.AngularVelocity != -999)
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
