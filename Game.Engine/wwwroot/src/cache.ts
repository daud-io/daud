import { spriteIndices } from "./spriteIndices";
import { RenderedObject } from "./renderedObject";
import { Fleet } from "./models/fleet";
import { GameContainer } from "./gameContainer";
import * as bus from "./bus";
import { Ship } from "./models/ship";
import { Token } from "./models/token";
import { Vector2 } from "@babylonjs/core";
import { NetBody } from "./daud-net/net-body";
import { NetGroup } from "./daud-net/net-group";
import { NetWorldView } from "./daud-net/net-world-view";

export type ClientBody = {
    ID: number;
    DefinitionTime: number;
    Size: number;
    Sprite: string;
    Mode: number;
    Color: string;
    Group: number;
    OriginalAngle: number;
    AngularVelocity: number;
    Momentum: Vector2;
    OriginalPosition: Vector2;
    zIndex: number;
    Position: Vector2;
    Angle: number;
};

export type ClientGroup = {
    ID: number;
    Caption: string | null;
    Type: number;
    ZIndex: number;
    CustomData: any;
    renderer?: Fleet;
};

type ClientRendered = {
    body: ClientBody;
    renderer?: RenderedObject;
};

export class Cache {
    static readonly VELOCITY_SCALE_FACTOR = 5000.0;
    readonly container: GameContainer;
    readonly bodies: Map<string, ClientRendered> = new Map();
    readonly groups: Map<string, ClientGroup> = new Map();

    constructor(container: GameContainer) {
        this.container = container;

        bus.on("worldjoin", () => this.onWorldJoin());
        bus.on("worldview", (view) => this.onWorldView(view));
    }

    onWorldView(newView: NetWorldView): void {
        const deletesLength = newView.deletesLength();
        for (let d = 0; d < deletesLength; d++) {
            const key = `b-${newView.deletes(d)!}`;
            const body = this.bodies.get(key);
            if (body) {
                const group = this.groups.get(`g-${body.body.Group}`);
                if (group && group.renderer) group.renderer.deleteShip(key);
                if (body.renderer) body.renderer.destroy();
            }

            this.bodies.delete(key);
        }

        const groupDeletesLength = newView.groupdeletesLength();
        for (let d = 0; d < groupDeletesLength; d++) {
            const key = `g-${newView.groupdeletes(d)!}`;
            const group = this.groups.get(key);
            if (!group) console.log("group delete on object not in cache");
            if (group && group.renderer) group.renderer.destroy();
            this.groups.delete(key);
        }

        const groupsLength = newView.groupsLength();
        for (let u = 0; u < groupsLength; u++) {
            let group = newView.groups(u)!;
            let existing = this.groups.get(`g-${group.group()}`);
            if (!existing) {
                const clientGroup = this.groupFromServer(group);
                if (clientGroup.Type == 1) clientGroup.renderer = new Fleet(this.container);
                this.groups.set(`g-${clientGroup.ID}`, clientGroup);
                existing = clientGroup;
            } else {
                existing.ID = group.group();
                existing.Caption = group.caption()!;
                existing.Type = group.type();
                existing.ZIndex = group.zindex();
                const cd = group.customdata();
                existing.CustomData = cd ? JSON.parse(cd) : cd;
            }

            if (existing.renderer) existing.renderer.update(existing, this.container.fleetID);
        }

        const updatesLength = newView.updatesLength();
        for (let u = 0; u < updatesLength; u++) {
            // update objects that should be here
            const update = newView.updates(u)!;
            const existing = this.bodies.get(`b-${update.id()}`);

            if (existing) {
                existing.body.Size = update.size() * 5;
                existing.body.Sprite = spriteIndices[update.sprite()];
                existing.body.Mode = update.mode();
                existing.body.DefinitionTime = update.definitiontime();
                existing.body.OriginalAngle = (update.originalangle() / 127) * Math.PI;
                existing.body.AngularVelocity = update.angularvelocity() / 10000;
                const originalPosition = update.originalposition()!;
                existing.body.OriginalPosition.x = originalPosition.x();
                existing.body.OriginalPosition.y = originalPosition.y();
                const velocity = update.velocity()!;
                existing.body.Momentum.x = velocity.x() / Cache.VELOCITY_SCALE_FACTOR;
                existing.body.Momentum.y = velocity.y() / Cache.VELOCITY_SCALE_FACTOR;

                if (update.group() != existing.body.Group) {
                    const oldGroup = this.groups.get(`g-${existing.body.Group}`);
                    if (oldGroup) oldGroup.renderer!.deleteShip(`b-${update.id()}`);
                    existing.body.Group = update.group();
                }

                existing.body.zIndex = 0;

                if (update.group() != 0) {
                    let group = this.groups.get(`g-${update.group()}`);
                    if (group) existing.body.zIndex = group.ZIndex || 0;
                }

                if (existing.renderer) existing.renderer.update();
            }

            if (!existing) {
                var renderer = <RenderedObject | undefined>undefined;
                const clientBody = Cache.bodyFromServer(update);

                const group = this.groups.get(`g-${clientBody.Group}`);

                clientBody.zIndex = group?.ZIndex || 0;

                const groupType = group?.Type ?? -1;

                switch (groupType) {
                    case 0: // fish
                        break;

                    case 1: // fleets
                        if (group?.renderer instanceof Fleet) {
                            var ship = new Ship(this.container, clientBody, group);
                            renderer = ship;
                            group.renderer.addShip(`b-${clientBody.ID}`, ship);
                        }
                        break;

                    case 6: // tokens
                        renderer = new Token(this.container, clientBody, group!);
                        break;
                }

                if (!renderer) renderer = new RenderedObject(this.container, clientBody);

                if (renderer) renderer.update();

                this.bodies.set(`b-${clientBody.ID}`, {
                    body: clientBody,
                    renderer,
                });
            }
        }

        this.container.updateCounter += updatesLength;
        this.container.viewCounter++;
    }

    onWorldJoin(): void {
        this.bodies.forEach((body) => {
            if (body && body.renderer) body.renderer.destroy();
        });

        this.groups.forEach((group) => {
            if (group && group.renderer) group.renderer.destroy();
        });

        this.bodies.clear();
        this.groups.clear();
    }

    refreshSprites(): void {
        this.bodies.forEach((body) => {
            if (body && body.renderer) body.renderer.refresh();
        });
    }

    tick(gameTime: number): void {
        this.bodies.forEach((body) => {
            if (body.renderer) body.renderer.tick(gameTime);
        });
        this.groups.forEach((group) => {
            if (group.renderer) group.renderer.tick(gameTime);
        });
    }

    groupFromServer(group: NetGroup): ClientGroup {
        let customData = group.customdata();
        if (customData) customData = JSON.parse(customData);

        return {
            ID: group.group(),
            Caption: group.caption(),
            Type: group.type(),
            ZIndex: group.zindex(),
            CustomData: customData,
        };
    }

    getGroup(groupID: number): ClientGroup | undefined {
        return this.groups.get(`g-${groupID}`);
    }

    static bodyFromServer(body: NetBody): ClientBody {
        const originalPosition = body.originalposition()!;
        const momentum = body.velocity()!;
        const groupID = body.group();

        const spriteIndex = body.sprite();
        const spriteName = spriteIndices[spriteIndex];

        return {
            ID: body.id(),
            DefinitionTime: body.definitiontime(),
            Size: body.size() * 5,
            Sprite: spriteName,
            Mode: body.mode(),
            Color: "red",
            Group: groupID,
            OriginalAngle: (body.originalangle() / 127) * Math.PI,
            AngularVelocity: body.angularvelocity() / 10000,
            Momentum: new Vector2(momentum.x() / Cache.VELOCITY_SCALE_FACTOR, momentum.y() / Cache.VELOCITY_SCALE_FACTOR),
            OriginalPosition: new Vector2(originalPosition.x(), originalPosition.y()),
            Position: new Vector2(0, 0),
            Angle: (body.originalangle() / 127) * Math.PI,
            zIndex: 0,
        };
    }
}
