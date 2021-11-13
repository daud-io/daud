namespace Game.API.Client
{
    using System.Collections.Generic;
    using System.Numerics;
    using DaudNet;
    using Game.API.Common;

    public class BodyCache
    {
        private readonly Dictionary<uint, Body> _bodies = new Dictionary<uint, Body>();
        private readonly Dictionary<uint, Group> _groups = new Dictionary<uint, Group>();


        private Vector2 FromNetVectorVelocity(Vec2 vec2)
        {
            var VELOCITY_SCALE_FACTOR = 5000.0f;

            return new Vector2
            {
                X = vec2.x / VELOCITY_SCALE_FACTOR,
                Y = vec2.y / VELOCITY_SCALE_FACTOR
            };

        }

        public void Clear()
        {
            _bodies.Clear();
            _groups.Clear();
        }

        public void Update(
            NetWorldView netWorldView
        )
        {
            for (int i = 0; i < netWorldView.updates.Count; i++)
            {
                var netBody = netWorldView.updates[i];
                var id = netBody.id;

                if (!_bodies.TryGetValue(netBody.id, out var body))
                {
                    body = new Body
                    {
                        ID = netBody.id
                    };
                    _bodies.Add(id, body);
                    body.Cache = this;
                }
                body.DefinitionTime = netBody.definitiontime;

                body.OriginalAngle = netBody.originalangle;
                body.AngularVelocity = netBody.angularvelocity;
                body.OriginalPosition.X = netBody.originalposition.x;
                body.OriginalPosition.Y = netBody.originalposition.y;
                body.Velocity = FromNetVectorVelocity(netBody.velocity);
                
                body.Size = netBody.size;
                body.Sprite = (Sprites)netBody.sprite;
                body.GroupID = netBody.group;
            }

            for (int i = 0; i < netWorldView.deletes.Count; i++)
            {
                var id = netWorldView.deletes[i];
                if (_bodies.ContainsKey(id))
                    _bodies.Remove(id);
            }

            for (int i = 0; i < netWorldView.groupdeletes.Count; i++)
            {
                var id = netWorldView.groupdeletes[i];
                if (_groups.ContainsKey(id))
                    _groups.Remove(id);
            }

            var groups = new List<Group>();
            for (int i = 0; i < netWorldView.groups.Count; i++)
            {
                var netGroup = netWorldView.groups[i];
                if (!_groups.TryGetValue(netGroup.group, out var group))
                {
                    group = new Group();
                    group.ID = netGroup.group;
                    _groups.Add(group.ID, group);
                }
                group.Caption = netGroup.caption;
                group.Type = (GroupTypes)netGroup.type;
                group.ZIndex = netGroup.zindex;
                group.Owner = netGroup.owner;
                group.Color = netGroup.color;
                group.CustomData = netGroup.customdata;
            }

            foreach (var body in Bodies)
                body.Project(netWorldView.time);
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
