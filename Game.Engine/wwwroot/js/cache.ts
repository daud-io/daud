import { Bullet } from "./models/bullet";
import { Ship } from "./models/ship";
import { RenderedObject } from "./models/renderedObject";
import { Fleet } from "./models/fleet";
import { Tile } from "./models/tile";
import { Container } from "pixi.js";
import { CustomContainer } from "./CustomContainer";

export class Cache {
    container: CustomContainer;
    bodies: any;
    groups: any;
    static count: number;
    constructor(container: CustomContainer) {
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
            let deleteKey = deletes[i];
            let key = `b-${deleteKey}`;
            if (key in this.bodies) Cache.count--;

            const body = this.bodies[key];
            if (body && body.renderer) body.renderer.destroy();
            delete this.bodies[key];
        }

        // delete groups that should no longer exist
        for (i = 0; i < groupDeletes.length; i++) {
            let deleteKey = groupDeletes[i];
            let key = `g-${deleteKey}`;
            let group = this.groups[key];
            if (!group) console.log("group delete on object not in cache");

            //console.log(`deleting group: ${key}`);

            if (group && group.renderer) group.renderer.destroy();
            delete this.groups[key];
        }

        // update groups that should be here
        for (i = 0; i < groups.length; i++) {
            const group = groups[i];
            let existing = this.groups[`g-${group.ID}`];

            if (!existing) {
                if (group.Type == 1) group.renderer = new Fleet(this.container, this);

                existing = group;
            } else {
                existing.ID = group.ID;
                existing.Caption = group.Caption;
                existing.Type = group.Type;
                existing.ZIndex = group.ZIndex;
                existing.CustomData = group.CustomData;
            }

            if (existing.renderer) existing.renderer.update(existing, myFleetID);

            this.groups[`g-${group.ID}`] = existing;
        }

        // update objects that should be here
        for (i = 0; i < updates.length; i++) {
            const update = updates[i];
            let existing = this.bodies[`b-${update.ID}`];

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

                let group = null;
                if (update.Group != 0) group = this.getGroup(update.Group);
                update.group = group;
                update.zIndex = 0;
                if (group)
                    update.zIndex = group.ZIndex || 0;

                if (update.renderer) update.renderer.update(update);
            }

            if (!existing) {
                let group = null;
                if (update.Group != 0) {
                    group = this.groups[`g-${update.Group}`];
                    if (group) {
                        switch (group.Type) {
                            case 1:
                                let ship = update.renderer;
                                if (!ship) ship = new Ship(this.container);
                                update.renderer = ship;

                                let fleet = group.renderer;
                                if (!fleet) fleet = new Fleet(this.container, this);
                                group.renderer = fleet;

                                if (fleet) fleet.addShip(ship);
                                break;

                            case 3:
                            case 4:
                                let bullet = update.renderer;
                                if (!bullet) bullet = new Bullet(this.container, this);
                                update.renderer = bullet;
                                break;

                            case 6:
                                let tile = update.renderer;
                                if (!tile) tile = new Tile(this.container, this);
                                update.renderer = tile;
                                break;
                        }
                    }
                }
                if (!update.renderer) update.renderer = new RenderedObject(this.container);

                update.group = group;
                update.zIndex = 0;
                if (group)
                    update.zIndex = group.ZIndex || 0;

                if (update.renderer) update.renderer.update(update, myFleetID);

                Cache.count++;
            }
        }
    }

    foreach(action, thisObj) {
        this.foreachGroup(function(group) {
            for (const key in this.bodies) {
                if (key.indexOf("b-") === 0) {
                    const body = this.bodies[key];
                    if (body.Group == group.ID) {
                        action.apply(thisObj, [body]);
                    }
                }
            }
        }, this);
    }

    foreachGroup(action, thisObj?) {
        const sortedGroups = [];

        for (const key in this.groups) {
            let group = this.groups[key];
            sortedGroups.push(group);
        }

        sortedGroups.sort((a, b) => a.ZIndex - b.ZIndex);
        sortedGroups.unshift({ ID: 0 });

        for (let group of sortedGroups) {
            action.apply(thisObj, [group]);
        }
    }

    getGroup(groupID) {
        return this.groups[`g-${groupID}`];
    }
}
