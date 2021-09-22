import { Settings } from "../settings";
import { GameContainer } from "../gameContainer";
import { ClientGroup, ClientBody } from "../cache";
import { Ship } from "./ship";
import { TextBlock } from "@babylonjs/gui";
import { Scene, Vector3 } from "@babylonjs/core";

export class Fleet {
    container: GameContainer;
    caption: string | null;
    ID?: number;
    text: TextBlock;
    chat?: string;
    ships: { [id: string]: Ship };
    plotly?: { data; layout };
    extraModes: string[];

    constructor(container: GameContainer) {
        this.container = container;
        this.caption = null;
        this.ID = undefined;
        this.ships = {};

        this.text = new TextBlock();
        this.text.color = "white";
        this.text.fontSize = Settings.nameSize / 3;
        this.container.guiTexture.addControl(this.text);

        this.extraModes = [];
    }

    addShip(id: string, ship: Ship): void {
        this.ships[id] = ship;
    }
    deleteShip(id: string): void {
        delete this.ships[id];
    }
    update(groupUpdate: ClientGroup, myFleetID: number): void {
        this.caption = groupUpdate.Caption;
        this.ID = groupUpdate.ID;

        this.chat = groupUpdate?.CustomData?.chat || undefined;
    }

    addPowerup(powerMode: string) {
        this.extraModes.push(powerMode);
        for (let id in this.ships) this.ships[id].updateTextureLayers();
    }
    removePowerup(powerMode: string) {
        this.extraModes = this.extraModes.filter((obj) => obj !== powerMode);
        for (let id in this.ships) this.ships[id].updateTextureLayers();
    }

    tick(time: number): void {
        //console.log(`Group: ${this.ID} ${this.caption} ${this.ships.length}`);
        this.text.text = this.caption || "";

        let accX = 0,
            accY = 0,
            count = 0;

        for (const shipkey in this.ships) {
            const ship = this.ships[shipkey];
            accX += ship.body.Position.x;
            accY += ship.body.Position.y;
            count++;
        }

        const offsetY = 0;
        this.text.moveToVector3(new Vector3(accX / count, 120, accY / count + offsetY), this.container.scene);
    }

    destroy(): void {
        this.container.guiTexture.removeControl(this.text);
    }
}

