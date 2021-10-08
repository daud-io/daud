namespace Game.API.Client
{
    using System.Collections.Generic;

    public class BodyCache
    {
        private readonly Dictionary<uint, Body> _bodies = new Dictionary<uint, Body>();
        private readonly Dictionary<uint, Group> _groups = new Dictionary<uint, Group>();

        public void Clear()
        {
            _bodies.Clear();
            _groups.Clear();
        }

        public void Update(
            IEnumerable<Body> updates,
            IEnumerable<uint> deletes,
            IEnumerable<Group> groups,
            IEnumerable<uint> groupDeletes,
            long time
        )
        {
            foreach (var id in deletes)
                if (_bodies.ContainsKey(id))
                    _bodies.Remove(id);

            foreach (var update in updates)
            {
                if (!_bodies.ContainsKey(update.ID))
                {
                    _bodies.Add(update.ID, update);
                    update.Cache = this;
                }
                else
                {
                    var existing = _bodies[update.ID];

                    existing.DefinitionTime = update.DefinitionTime;

                    existing.OriginalAngle = update.OriginalAngle;
                    existing.AngularVelocity = update.AngularVelocity;
                    existing.OriginalPosition = update.OriginalPosition;
                    existing.Velocity = update.Velocity;
                    
                    existing.Size = update.Size;
                    existing.Sprite = update.Sprite;
                    existing.GroupID = update.GroupID;
                }
            }

            foreach (var id in groupDeletes)
                if (_groups.ContainsKey(id))
                    _groups.Remove(id);

            foreach (var group in groups)
            {
                if (!_groups.ContainsKey(group.ID))
                    _groups.Add(group.ID, group);
                else
                {
                    var existing = _groups[group.ID];
                    existing.Color = group.Color;
                    existing.Caption = group.Caption;
                    existing.Type = group.Type;
                    existing.ZIndex = group.ZIndex;
                }
            }

            foreach (var body in Bodies)
                body.Project(time);
        }

        public IEnumerable<Body> Bodies
        {
            get
            {
                return this._bodies.Values;
            }
        }

        public Group GetGroup(uint group)
        {
            return _groups.ContainsKey(group)
                ? _groups[group]
                : null;
        }
    }
}
