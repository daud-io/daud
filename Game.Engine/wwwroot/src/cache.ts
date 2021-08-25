import { spriteIndices } from "./spriteIndices";
import { NetGroup, NetBody } from "./game_generated";
import { RenderedObject } from "./renderedObject";
import { Fleet } from "./models/fleet";
import { GameContainer } from "./gameContainer";
import bus from "./bus";
import { Ship } from "./models/ship";
import { Token } from "./models/token";
import { Vector2 } from "@babylonjs/core";

type ClientRendered = {
    body: ClientBody;
    renderer?: RenderedObject;
};
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
    obsolete?: number;
    Position: Vector2;
    Angle: number;
};

const VELOCITY_SCALE_FACTOR = 5000.0;
export function bodyFromServer(body: NetBody): ClientBody {
    const originalPosition = body.originalPosition()!;
    const momentum = body.velocity()!;
    const groupID = body.group();

    const spriteIndex = body.sprite();
    const spriteName = spriteIndices[spriteIndex];

    return {
        ID: body.id(),
        DefinitionTime: body.definitionTime(),
        Size: body.size() * 5,
        Sprite: spriteName,
        Mode: body.mode(),
        Color: "red",
        Group: groupID,
        OriginalAngle: (body.originalAngle() / 127) * Math.PI,
        AngularVelocity: body.angularVelocity() / 10000,
        Momentum: new Vector2(momentum.x() / VELOCITY_SCALE_FACTOR, momentum.y() / VELOCITY_SCALE_FACTOR),
        OriginalPosition: new Vector2(originalPosition.x(), originalPosition.y()),
        Position: new Vector2(0, 0),
        Angle: (body.originalAngle() / 127) * Math.PI,
        zIndex: 0,
    };
}

export type ClientGroup = {
    ID: number;
    Caption: string | null;
    Type: number;
    ZIndex: number;
    CustomData: any;
    renderer?: Fleet;
};
export function groupFromServer(group: NetGroup): ClientGroup {
    let customData = group.customData();
    if (customData) customData = JSON.parse(customData);

    return {
        ID: group.group(),
        Caption: group.caption(),
        Type: group.type(),
        ZIndex: group.zindex(),
        CustomData: customData,
    };
}

let container: GameContainer | undefined;
const bodies: Map<string, ClientRendered> = new Map();
const groups: Map<string, ClientGroup> = new Map();

export function getGroup(groupID: number): ClientGroup | undefined
{
    return groups.get(`g-${groupID}`);
}

export function setContainer(newContainer: GameContainer): void {
    container = newContainer;
    if (groups.size > 0 || bodies.size > 0)
        throw "container change";
}

bus.on("worldjoin", () => {
    bodies.forEach((body) => {
        if (body && body.renderer) body.renderer.destroy();
    });

    groups.forEach((group) => {
        if (group && group.renderer) group.renderer.destroy();
    });

    bodies.clear();
    groups.clear();
});

export function refreshSprites(): void {
    bodies.forEach((body) => {
        if (body && body.renderer) body.renderer.refresh();
    });
}
export function update(updates: NetBody[], deletes: number[], newGroups: NetGroup[], groupDeletes: number[], time: number, myFleetID: number): void {
    if (!container)
        throw "update before container";

    // delete objects that should no longer exist
    for (const deleteKey of deletes) {
        const key = `b-${deleteKey}`;

        const body = bodies.get(key);
        if (body) {
            const group = groups.get(`g-${body.body.Group}`);
            if (group && group.renderer) group.renderer.deleteShip(key);
            if (body.renderer) body.renderer.destroy();
        }

        bodies.delete(key);
    }

    // delete groups that should no longer exist
    for (const deleteKey of groupDeletes) {
        const key = `g-${deleteKey}`;
        const group = groups.get(key);
        if (!group) console.log("group delete on object not in cache");

        // console.log(`deleting group: ${key}`);

        if (group && group.renderer) group.renderer.destroy();
        groups.delete(key);
    }

    // update groups that should be here
    for (const group of newGroups) {
        let existing = groups.get(`g-${group.group()}`);

        if (!existing) {
            const clientGroup = groupFromServer(group);
            if (clientGroup.Type == 1) clientGroup.renderer = new Fleet(container);
            groups.set(`g-${clientGroup.ID}`, clientGroup);
            existing = clientGroup;
        } else {
            existing.ID = group.group();
            existing.Caption = group.caption()!;
            existing.Type = group.type();
            existing.ZIndex = group.zindex();
            const cd = group.customData();
            existing.CustomData = cd ? JSON.parse(cd) : cd;
        }

        if (existing.renderer) existing.renderer.update(existing, myFleetID);
    }

    // update objects that should be here
    for (const update of updates) {
        const existing = bodies.get(`b-${update.id()}`);

        if (existing) {
            existing.body.obsolete = time;
            existing.body.Size = update.size() * 5;
            existing.body.Sprite = spriteIndices[update.sprite()];
            existing.body.Mode = update.mode();
            existing.body.DefinitionTime = update.definitionTime();
            existing.body.OriginalAngle = (update.originalAngle() / 127) * Math.PI;
            existing.body.AngularVelocity = update.angularVelocity() / 10000;
            const originalPosition = update.originalPosition()!;
            existing.body.OriginalPosition.x = originalPosition.x();
            existing.body.OriginalPosition.y = originalPosition.y();
            const velocity = update.velocity()!;
            existing.body.Momentum.x = velocity.x() / VELOCITY_SCALE_FACTOR;
            existing.body.Momentum.y = velocity.y() / VELOCITY_SCALE_FACTOR;

            if (update.group() != existing.body.Group) {
                const oldGroup = groups.get(`g-${existing.body.Group}`);
                if (oldGroup) oldGroup.renderer!.deleteShip(`b-${update.id()}`);
                existing.body.Group = update.group();
            }

            existing.body.zIndex = 0;

            if (update.group() != 0)
            {
                let group = groups.get(`g-${update.group()}`);  
                if (group) existing.body.zIndex = group.ZIndex || 0;
            } 

            if (existing.renderer) existing.renderer.update();
        }

        if (!existing) {
            var renderer = <RenderedObject|undefined>undefined;
            const clientBody = bodyFromServer(update);

            const group = groups.get(`g-${clientBody.Group}`);

            clientBody.zIndex = group?.ZIndex || 0;

            const groupType = group?.Type ?? -1;

            switch (groupType)
            {
                case 0: // fish
                    break;

                case 1: // fleets
                    if (group?.renderer instanceof Fleet)
                    {
                        var ship = new Ship(container, clientBody, group);
                        renderer = ship;
                        group.renderer.addShip(`b-${clientBody.ID}`, ship);
                    }
                    break;

                case 6: // tokens
                    renderer = new Token(container, clientBody, group!);
                    break;

            }

            if (!renderer) renderer = new RenderedObject(container, clientBody);
            
            if (renderer) renderer.update();

            bodies.set(`b-${clientBody.ID}`, {
                body: clientBody,
                renderer,
            });
        }

    }
}

export function tick(gameTime: number): void {
    bodies.forEach((body) => {
        if (body.renderer) body.renderer.tick(gameTime);
    });
    groups.forEach((group) => {
        if (group.renderer) group.renderer.tick(gameTime);
    });
}
