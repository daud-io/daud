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
import { Vec2 } from "./daud-net/vec2";

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
    Velocity: Vector2;
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
    lastUpdate: NetBody;
    group?: ClientGroup;
    body: ClientBody;
    renderer?: RenderedObject;
};

export class Cache {
    static readonly VELOCITY_SCALE_FACTOR = 5000.0;
    readonly container: GameContainer;

    bodyKeys: Number[] = [];
    groupKeys: Number[] = [];
    bodies: Array<ClientRendered> = [];
    groups: Array<ClientGroup> = [];

    messageBuffers = {
        group: new NetGroup(),
        tmpVec2: new Vec2(),
        tmpVec2Compare: new Vec2(),
        update: new NetBody()
    };

    constructor(container: GameContainer) {
        this.container = container;

        bus.on("disconnected", () => this.onDisconnected());
        bus.on("worldview", (view) => this.onWorldView(view));
    }

    deleteBody(key:number)
    {
        const index = this.bodyKeys.indexOf(key);
        if (index > -1)
        {
            this.bodyKeys.splice(index, 1);
            this.bodies.splice(index, 1);
        }
    }

    deleteGroup(key:number)
    {
        const index = this.groupKeys.indexOf(key);
        if (index > -1)
        {
            this.groupKeys.splice(index, 1);
            this.groups.splice(index, 1);
        }
    }

    addGroup(id: number, clientGroup: ClientGroup) {
        this.groups.push(clientGroup);
        this.groupKeys.push(id);
    }

    addBody(id: number, clientRendered: ClientRendered) {
        this.bodies.push(clientRendered);
        this.bodyKeys.push(id);
    }

    getBody(id: number): ClientRendered|undefined
    {
        const index = this.bodyKeys.indexOf(id);
        if (index > -1)
            return this.bodies[index];
        else
            return undefined;
    }

    getGroup(id: number): ClientGroup|undefined
    {
        const index = this.groupKeys.indexOf(id);
        if (index > -1)
            return this.groups[index];
        else
            return undefined;
    }

    onWorldView(newView: NetWorldView): void {

        const deletesLength = newView.deletesLength();
        for (let d = 0; d < deletesLength; d++) {
            const key = newView.deletes(d)!;
            const body = this.getBody(key);
            if (body) {
                const group = this.getGroup(body.body.Group);
                group?.renderer?.deleteShip(key);
                body.renderer?.dispose();
            }

            this.deleteBody(key);
        }

        const groupDeletesLength = newView.groupdeletesLength();
        for (let d = 0; d < groupDeletesLength; d++) {
            const key = newView.groupdeletes(d)!;
            //console.log(`delete group: ${key}`)
            const group = this.getGroup(key);
            if (!group) console.log("group delete on object not in cache");
            if (group && group.renderer) group.renderer.dispose();

            this.deleteGroup(key);
        }

        const groupsLength = newView.groupsLength();
        const group:NetGroup = this.messageBuffers.group;

        for (let u = 0; u < groupsLength; u++) {
            newView.groups(u, group)!;
            let existing = this.getGroup(group.group());
            if (!existing) {
                //console.log(`new group ${group.group()} ${group.type()} ${group.caption()}`);
                const clientGroup = this.groupFromServer(group);
                if (clientGroup.Type == 1)
                    clientGroup.renderer = new Fleet(this.container);
                
                this.addGroup(clientGroup.ID, clientGroup);
                existing = clientGroup;
            } else {
                existing.Caption = group.caption()!;
                existing.ZIndex = group.zindex();
            }

            existing.renderer?.update(existing, this.container.fleetID);
        }

        const updatesLength = newView.updatesLength();
        const update:NetBody = this.messageBuffers.update;
        const tmpVec2:Vec2  = this.messageBuffers.tmpVec2;
        const tmpVec2Compare:Vec2 = this.messageBuffers.tmpVec2Compare;

        for (let u = 0; u < updatesLength; u++) {
            // update objects that should be here
            newView.updates(u, update)!;
            const existing = this.getBody(update.id());

            if (existing) {

                const lastUpdate = existing.lastUpdate;

                if (update.size() != lastUpdate.size())
                    existing.body.Size = update.size() * 5;

                if (update.sprite() != lastUpdate.sprite())
                    existing.body.Sprite = spriteIndices[update.sprite()];

                existing.body.Mode = update.mode();
                existing.body.DefinitionTime = update.definitiontime();
                if (update.originalangle() != lastUpdate.originalangle())
                    existing.body.OriginalAngle = (update.originalangle() / 127) * Math.PI;

                if (update.angularvelocity() != lastUpdate.angularvelocity())
                    existing.body.AngularVelocity = update.angularvelocity() / 10000;

                update.originalposition(tmpVec2)!;
                existing.body.OriginalPosition.x = tmpVec2.x();
                existing.body.OriginalPosition.y = tmpVec2.y();

                update.velocity(tmpVec2);
                lastUpdate.velocity(tmpVec2Compare);
                
                if (tmpVec2.x() != tmpVec2Compare.x()
                    || tmpVec2.y() != tmpVec2Compare.y())
                {
                    existing.body.Velocity.x = tmpVec2.x() / Cache.VELOCITY_SCALE_FACTOR;
                    existing.body.Velocity.y = tmpVec2.y() / Cache.VELOCITY_SCALE_FACTOR;
                }

                if (update.group() != existing.body.Group) {
                    const oldGroup = existing.group;
                    if (oldGroup && oldGroup.renderer)
                        oldGroup.renderer.deleteShip(update.id());

                    existing.body.Group = update.group();

                    if (update.group() != 0)
                        existing.group = this.getGroup(update.group());
                    else
                        existing.group = undefined;
    
                }

                if (existing.group != null)
                    existing.body.zIndex = existing.group.ZIndex || 0;

                existing.renderer?.update();

                newView.updates(u, existing.lastUpdate);
            }

            if (!existing) {
                var renderer = <RenderedObject | undefined>undefined;
                const clientBody = Cache.bodyFromServer(update);

                const group = this.getGroup(clientBody.Group);
                if (clientBody.Group != 0 && group == undefined)
                {
                    console.log('missing group while adding body');
                }

                clientBody.zIndex = group?.ZIndex || 0;

                const groupType = group?.Type ?? -1;

                switch (groupType) {
                    case 1: // fleets
                        if (group?.renderer instanceof Fleet) {
                            var ship = new Ship(this.container, clientBody, group);
                            renderer = ship;
                            group.renderer.addShip(clientBody.ID, ship);
                        }
                        break;

                    case 6: // tokens
                        renderer = new Token(this.container, clientBody, group!);
                        break;
                    
                    case 0: // fish
                    default:
                        renderer = new RenderedObject(this.container, clientBody);
                        break;
                }
                
                renderer?.update();
                this.addBody(clientBody.ID, {
                    lastUpdate: newView.updates(u)!,
                    body: clientBody,
                    renderer,
                });
            }
        }

        this.container.updateCounter += updatesLength;
        this.container.viewCounter++;
        this.container.connection.cacheSize = this.bodies.length;

        //console.log(this.groups.size);
    }
    onDisconnected(): void {
        console.log('clearing cache');
        for(let i=0; i<this.bodies.length; i++)
            this.bodies[i]?.renderer?.dispose();

        for(let i=0; i<this.groups.length; i++)
            this.groups[i]?.renderer?.dispose();

        this.bodies.length = 0;
        this.groups.length = 0;
        this.bodyKeys.length = 0;
        this.groupKeys.length = 0;
    }

    refreshSprites(): void {
        for(let i=0; i<this.bodies.length; i++)
            this.bodies[i]?.renderer?.refresh();
    }

    tick(gameTime: number): void {
        for(let i=0; i<this.bodies.length; i++)
            this.bodies[i].renderer?.tick(gameTime);
        for(let i=0; i<this.groups.length; i++)
            this.groups[i].renderer?.tick(gameTime);
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

    static bodyFromServer(body: NetBody): ClientBody {
        const originalPosition = body.originalposition()!;
        const velocity = body.velocity()!;
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
            Velocity: new Vector2(velocity.x() / Cache.VELOCITY_SCALE_FACTOR, velocity.y() / Cache.VELOCITY_SCALE_FACTOR),
            OriginalPosition: new Vector2(originalPosition.x(), originalPosition.y()),
            Position: new Vector2(0, 0),
            Angle: (body.originalangle() / 127) * Math.PI,
            zIndex: 0,
        };
    }

    static vectorBuffer: Vec2 = new Vec2();
    static positionFromServerBody(body: NetBody, position: Vector2): void {
        body.originalposition(Cache.vectorBuffer)!;
        position.x = Cache.vectorBuffer.x();
        position.y = Cache.vectorBuffer.y();
    }
}
