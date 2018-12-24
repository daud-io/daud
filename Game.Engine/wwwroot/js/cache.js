import { sprites } from "./renderer";

export class Cache {
    constructor(container) {
        this.container = container;
        this.clear();
    }

    clear() {
        this.bodies = {};
        this.groups = {};
        Cache.count = 0;
    }

    update(updates, deletes, groups, groupDeletes, time) {
        let i = 0;

        // delete objects that should no longer exist
        for (i = 0; i < deletes.length; i++) {
            var deleteKey = deletes[i];
            var key = `b-${deleteKey}`;
            this.container.removeChild(this.bodies[`p-${deleteKey}`]);
            if (key in this.bodies) Cache.count--;
            delete this.bodies[key];
        }

        // delete groups that should no longer exist
        for (i = 0; i < groupDeletes.length; i++) {
            var deleteKey = groupDeletes[i];
            var key = `g-${deleteKey}`;
            delete this.groups[key];
        }

        // update objects that should be here
        for (i = 0; i < updates.length; i++) {
            const update = updates[i];
            var existing = this.bodies[`b-${update.ID}`];

            this.bodies[`b-${update.ID}`] = update;

            if (existing) {
                existing.previous = false;
                existing.obsolete = time;
                update.previous = existing;

                if (update.Size === -1) update.Size = existing.Size;

                if (update.Sprite === null) update.Sprite = existing.Sprite;
                if (update.Caption === null) update.Caption = existing.Caption;
                if (update.Color === null) update.Color = existing.Color;

                if (update.OriginalAngle === -999) update.OriginalAngle = existing.OriginalAngle;
                if (update.AngularVelocity === -999) update.AngularVelocity = existing.AngularVelocity;

                let sprite = sprites[update.Sprite];
                let object = this.bodies[`p-${update.ID}`];
                object.position.x = update.OriginalPosition.X;
                object.position.y = update.OriginalPosition.Y;
                object.rotation = update.OriginalAngle;
                object.scale.set(sprite.scale * update.Size, sprite.scale * update.Size);
            }

            if (!existing) {
                let sprite = sprites[update.Sprite];
                let texture = new PIXI.Texture.fromLoader(sprite.image);
                var object = new PIXI.Sprite(texture);

                object.position.x = update.OriginalPosition.X;
                object.position.y = update.OriginalPosition.Y;
                object.pivot.x = sprite.image.width / 2;
                object.pivot.y = sprite.image.height / 2;
                object.rotation = update.OriginalAngle;
                object.scale.set(sprite.scale * update.Size, sprite.scale * update.Size);

                this.container.addChild(object);
                this.bodies[`p-${update.ID}`] = object;
                Cache.count++;
            }
        }

        // update groups that should be here
        for (i = 0; i < groups.length; i++) {
            const group = groups[i];
            var existing = this.groups[`g-${group.ID}`];

            if (!existing) existing = group;
            else {
                existing.ID = group.ID;
                existing.Caption = group.Caption;
                existing.Type = group.Type;
                existing.ZIndex = group.ZIndex;
            }

            this.groups[`g-${group.ID}`] = existing;
        }
    }

    foreach(action, thisObj) {
        const sortedGroups = [];

        for (var key in this.groups) {
            var group = this.groups[key];
            sortedGroups.push(group);
        }

        sortedGroups.sort((a, b) => a.ZIndex - b.ZIndex);
        sortedGroups.unshift({ ID: 0 });

        for (let g = 0; g < sortedGroups.length; g++) {
            var group = sortedGroups[g];

            for (var key in this.bodies) {
                if (key.indexOf("b-") === 0) {
                    const body = this.bodies[key];
                    if (body.Group == group.ID) {
                        action.apply(thisObj, [body]);
                    }
                }
            }
        }
    }

    getGroup(groupID) {
        return this.groups[`g-${groupID}`];
    }
}
