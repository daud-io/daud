import { Settings } from "./settings";
import * as PIXI from "pixi.js";
import { CustomContainer } from "./CustomContainer";
import Plotly from "./plotly-subset";
import { projectObject } from "./interpolator";
import { ClientGroup, ClientBody } from "./cache";

export class Fleet {
    container: CustomContainer;
    caption: string | null;
    ID?: number;
    text: PIXI.Text;
    textChat: PIXI.Text;
    chat?: string;
    ships: { [id: string]: ClientBody };
    plotly?: { data; layout };
    usingPlotly: boolean;

    constructor(container: CustomContainer) {
        this.container = container;
        this.caption = null;
        this.ID = undefined;
        this.ships = {};
        this.text = new PIXI.Text("", { fontFamily: ["Exo 2", "Noto Color Emoji"], fontSize: Settings.nameSize, fill: 0xffffff });
        this.textChat = new PIXI.Text("", { fontFamily: ["Exo 2", "Noto Color Emoji"], fontSize: Settings.nameSize, fill: 0xffffff });
        this.text.zIndex = 1000;
        this.textChat.zIndex = 1000;
        this.chat = undefined;
        this.plotly = undefined;
        this.text.anchor.set(0.5, 0.5);
        this.textChat.anchor.set(0.5, 0.5);
        this.text.position.x = 0;
        this.text.position.y = 0;
        this.textChat.position.x = 0;
        this.textChat.position.y = 0;
        this.container.bodyGroup.addChild(this.text);
        this.container.bodyGroup.addChild(this.textChat);
        this.usingPlotly = false;
    }

    addShip(id: string, ship: ClientBody): void {
        this.ships[id] = ship;
    }
    deleteShip(id: string): void {
        delete this.ships[id];
    }
    update(groupUpdate: ClientGroup, myFleetID: number): void {
        this.caption = groupUpdate.Caption;
        this.ID = groupUpdate.ID;

        this.chat = groupUpdate?.CustomData?.chat || undefined;
        this.plotly = groupUpdate?.CustomData?.plotly || undefined;

        if (this.plotly && this.ID == myFleetID) {
            if (!this.container.plotly.used) {
                this.container.plotly.used = true;
                this.usingPlotly = true;
                console.log("setting plotly use");
            }
            Plotly.react(this.container.plotly, this.plotly.data, this.plotly.layout, {
                displayModeBar: false,
                staticPlot: true,
            });
        }

        if (this.usingPlotly && this.ID != myFleetID) {
            // we must have been spectating a fleet
            // with plotly data, and now we've switched
            // to a different fleet to follow
            // but the original one is still on screen
            // ... that's us.
            this.container.plotly.used = false;
            this.usingPlotly = false;
        }
    }

    tick(time: number): void {
        //console.log(`Group: ${this.ID} ${this.caption} ${this.ships.length}`);
        this.text.text = this.caption || "";
        this.textChat.text = this.chat || "";

        let accX = 0,
            accY = 0,
            count = 0;

        for (const shipkey in this.ships) {
            const ship = this.ships[shipkey];
            projectObject(ship, time);
            accX += ship.Position.x;
            accY += ship.Position.y;
            count++;
        }

        const offsetY = 0;
        this.text.position.x = accX / count;
        this.text.position.y = accY / count + offsetY;
        this.textChat.position.x = accX / count;
        this.textChat.position.y = accY / count + offsetY - 200;
    }

    destroy(): void {
        this.container.bodyGroup.removeChild(this.text);
        this.container.bodyGroup.removeChild(this.textChat);
        if (this.usingPlotly) {
            this.container.plotly.used = false;
            console.log("unsetting plotly use");
        }
    }
}
