import { CameraModes, GameContainer } from "./gameContainer";
import * as bus from "./bus";
import { LeaderboardType } from "./connection";
import { Leaderboard } from "./leaderboard";
import { Vector2 } from "@babylonjs/core";
import { RenderedObject } from "./renderedObject";
import { Ship, ShipModes } from "./models/ship";

type MapObject =
    {
        isSelf: boolean;
        name: string | undefined;
        position: Vector2;
        stale: Boolean;
        renderedObject: RenderedObject;
    }

export class Supermap {
    container: GameContainer;
    mapObjects: Record<string, MapObject>;
    visible: boolean = false;

    constructor(container: GameContainer) {
        this.container = container;

        this.mapObjects = {};

        bus.on('prerender', (time) => this.prerender(time));
        bus.on('mapShow', () => this.show());
        bus.on('mapHide', () => this.hide());
        bus.on("leaderboard", (lb) => {
            this.update(lb);
        });

        bus.on('disconnected', () => this.clear());
    }

    clear(staleOnly: boolean = false): void {
        const keys: string[] = [];

        for (var key in this.mapObjects)
            if (!staleOnly || this.mapObjects[key].stale)
                keys.push(key);

        for (var key in keys)
            this.deleteMapObject(key);
    }

    deleteMapObject(key: string) {
        console.log('deleting obj');
        const obj = this.mapObjects[key];
        obj?.renderedObject?.dispose();
        delete this.mapObjects[key];
    }

    update(data: LeaderboardType): void {

        for (var key in this.mapObjects) {
            var obj = this.mapObjects[key];
            obj.stale = true;
        }
        switch (data.Type) {
            case "FFA":
                for (let i = 0; i < data.Entries.length; i++) {
                    const entryIsSelf = data.Entries[i].FleetID == this.container.fleetID;
                    const entry = data.Entries[i];

                    var mapObjectID = `f${entry.FleetID}`;

                    let mapObj: MapObject = this.mapObjects[mapObjectID] ?? {};
                    mapObj.isSelf = entryIsSelf;
                    mapObj.name = entry.Name;
                    mapObj.position = entry.Position;
                    mapObj.stale = false;

                    if (!mapObj.renderedObject)
                        mapObj.renderedObject = new RenderedObject(this.container,
                            {
                                ID: 0,
                                DefinitionTime: this.container.lastGameTime,
                                Size: 300,
                                Sprite: 'ship_' + entry.Color,
                                Mode: ShipModes.offenseupgrade,
                                Color: entry.Color,
                                Group: 0,
                                OriginalAngle: -Math.PI / 2,
                                AngularVelocity: 0,
                                Velocity: Vector2.Zero(),
                                OriginalPosition: entry.Position,
                                Position: Vector2.Zero(),
                                Angle: 0,
                                zIndex: 300
                            });



                    mapObj.renderedObject.body.OriginalPosition = entry.Position;
                    mapObj.renderedObject.body.OriginalAngle = -Math.PI / 2;
                    mapObj.renderedObject.visible = this.visible;
                    mapObj.renderedObject.update();
                    mapObj.renderedObject.tick(this.container.lastGameTime);
                    this.mapObjects[mapObjectID] = mapObj;
                }

                break;

            case "Team":
                break;
            case "CTF":
                break;
        }

        for (var key in this.mapObjects) {
            var obj = this.mapObjects[key];
            if (obj.stale)
                this.deleteMapObject(key);
        }        
    }

    show() {
        this.container.cameraMode = CameraModes.Supermap;
        this.visible = true;
        this.updateObjects();
        console.log('show');
    }

    updateObjects()
    {
        for (var key in this.mapObjects) {
            var obj = this.mapObjects[key];
            obj.renderedObject.visible = this.visible;
            obj.renderedObject.update();
            obj.renderedObject.tick(this.container.lastGameTime);
        }

    }

    hide() {
        console.log('hide');

        this.container.cameraMode = CameraModes.Default;
        this.visible = false;
        this.updateObjects();
        
    }

    prerender(time: number) {
    }
}

