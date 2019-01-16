import { Bullet } from "./models/bullet";
import { Ship } from "./models/ship";
import { RenderedObject } from "./models/renderedObject";

export class Cache {
    constructor(container) {
        this.container = container;
        this.clear();
    }

    clear() {
        this.foreach(function(body) {
            if (body && body.renderer)
                body.renderer.destroy();
        }, this);

        this.bodies = {};
        this.groups = {};
        Cache.count = 0;
    }

    empty() {
        this.clear();
    }

    refreshSprites() {
        // I used to loop all the bodies and reset the textures;
    }

    update(updates, deletes, groups, groupDeletes, time) {
        let i = 0;

        // delete objects that should no longer exist
        for (i = 0; i < deletes.length; i++) {
            var deleteKey = deletes[i];
            var key = `b-${deleteKey}`;
            if (key in this.bodies) Cache.count--;

            var body = this.bodies[key];
            if (body && body.renderer)
                body.renderer.destroy();
            delete this.bodies[key];
        }

        // delete groups that should no longer exist
        for (i = 0; i < groupDeletes.length; i++) {
            var deleteKey = groupDeletes[i];
            this.container.removeChild(this.bodies[`p-${deleteKey}`]);
            delete this.bodies[`p-${deleteKey}`];
            var key = `g-${deleteKey}`;
            delete this.groups[key];
        }

        // update objects that should be here
        for (i = 0; i < updates.length; i++) {
            const update = updates[i];
            var existing = this.bodies[`b-${update.ID}`];
            
            this.bodies[`b-${update.ID}`] = update;

            if (existing) {
                update.renderer = existing.renderer;
                update.previous = existing;

                existing.previous = false;
                existing.renderer = false;
                existing.obsolete = time;

                if (update.Size === -1) update.Size = existing.Size;

                if (update.Sprite === null) update.Sprite = existing.Sprite;

                if (update.OriginalAngle === -999) update.OriginalAngle = existing.OriginalAngle;
                if (update.AngularVelocity === -999) update.AngularVelocity = existing.AngularVelocity;

                if (update.renderer)
                    update.renderer.update(update);
            }

            if (!existing) {
                if (update.Sprite.indexOf("ship") == 0)
                    update.renderer = new Ship(this.container);
                else if (update.Sprite.indexOf("bullet"))
                    update.renderer = new Bullet(this.container);
                else
                    update.renderer = new RenderedObject(this.container);

                update.renderer.update(update);
                Cache.count++;
            }
        }

        // update groups that should be here
        for (i = 0; i < groups.length; i++) {
            const group = groups[i];
            var existing = this.groups[`g-${group.ID}`];

            if (!existing) {
                /*let text = new PIXI.Text(group.Caption, { fontFamily: Settings.font, fontSize: Settings.nameSize, fill: 0xffffff });
                text.anchor.set(0.5, 0.5);
                this.container.addChild(text);
                this.bodies[`p-${group.ID}`] = text;
                if (!Settings.namesEnabled) text.visible = false;*/
                existing = group;
            } else {
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
