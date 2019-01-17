import { Bullet } from "./models/bullet";
import { Ship } from "./models/ship";
import { RenderedObject } from "./models/renderedObject";
import { Fleet } from "./models/fleet";

export class Cache {
    constructor(container) {
        this.container = container;
        this.clear();
    }

    clear() {
        this.foreach(function(body) {
            if (body && body.renderer) body.renderer.destroy();
        }, this);

        this.foreachGroup(function(group) {
            if (group && group.renderer) group.renderer.destroy();
        });

        this.bodies = {};
        this.groups = {};
        Cache.count = 0;
    }

    empty() {
        this.clear();
    }

    refreshSprites() {
        this.foreach(function(body) {
            if (body && body.renderer) body.renderer.refreshSprite();
        }, this);
    }

    update(updates, deletes, groups, groupDeletes, time, myFleetID) {
        let i = 0;

        // delete objects that should no longer exist
        for (i = 0; i < deletes.length; i++) {
            var deleteKey = deletes[i];
            var key = `b-${deleteKey}`;
            if (key in this.bodies) Cache.count--;

            var body = this.bodies[key];
            if (body && body.renderer) body.renderer.destroy();
            delete this.bodies[key];
        }

        // delete groups that should no longer exist
        for (i = 0; i < groupDeletes.length; i++) {
            var deleteKey = groupDeletes[i];
            var key = `g-${deleteKey}`;
            var group = this.groups[key];
            if (!group) console.log("group delete on object not in cache");

            if (group && group.renderer) group.renderer.destroy();
            delete this.groups[key];
        }

        // update groups that should be here
        for (i = 0; i < groups.length; i++) {
            const group = groups[i];
            var existing = this.groups[`g-${group.ID}`];

            if (!existing) {
                if (group.Type == 1) group.renderer = new Fleet(this.container, this);

                existing = group;
            } else {
                existing.ID = group.ID;
                existing.Caption = group.Caption;
                existing.Type = group.Type;
                existing.ZIndex = group.ZIndex;
            }

            if (existing.renderer) existing.renderer.update(existing);

            this.groups[`g-${group.ID}`] = existing;
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

                if (update.renderer) update.renderer.update(update);
            }

            if (!existing) {
                if (update.Sprite.indexOf("ship") == 0) {
                    var fleet = false;
                    if (update.Group != 0) {
                        var group = this.groups[`g-${update.Group}`];
                        if (!group) {
                            console.log("missing group");
                        } else {
                            if (group.Type == 1) {
                                fleet = group.renderer;

                                if (!fleet) fleet = new Fleet(this.container, this);

                                group.renderer = fleet;
                            }
                        }
                    } else {
                        console.log("ship with no group: " + update.Sprite);
                    }

                    var ship = (update.renderer = new Ship(this.container));

                    if (fleet) fleet.addShip(ship);
                } else if (update.Sprite.indexOf("bullet")) update.renderer = new Bullet(this.container);
                else update.renderer = new RenderedObject(this.container);

                update.renderer.update(update);
                Cache.count++;
            }
        }
    }

    foreach(action, thisObj) {
        this.foreachGroup(function(group) {
            for (var key in this.bodies) {
                if (key.indexOf("b-") === 0) {
                    const body = this.bodies[key];
                    if (body.Group == group.ID) {
                        action.apply(thisObj, [body]);
                    }
                }
            }
        }, this);
    }

    foreachGroup(action, thisObj) {
        const sortedGroups = [];

        for (var key in this.groups) {
            var group = this.groups[key];
            sortedGroups.push(group);
        }

        sortedGroups.sort((a, b) => a.ZIndex - b.ZIndex);
        sortedGroups.unshift({ ID: 0 });

        for (let g = 0; g < sortedGroups.length; g++) {
            var group = sortedGroups[g];
            action.apply(thisObj, [group]);
        }
    }

    getGroup(groupID) {
        return this.groups[`g-${groupID}`];
    }
}
