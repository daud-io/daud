import Plotly from "../plotly-subset";
import { Settings } from "../settings";
import "pixi.js";
import { CustomContainer } from "../CustomContainer";

export class Fleet {
    container: CustomContainer;
    caption?: string;
    ships: any[];
    ID: boolean;
    text: PIXI.Text;
    textChat: PIXI.Text;
    chat?: string;
    plotly?: { data; layout };
    usingPlotly: boolean;
    constructor(container: CustomContainer, cache) {
        this.container = container;
        //this.container.sortableChildren = true;
        this.caption = null;
        this.ships = [];
        this.ID = false;
        this.text = new PIXI.Text("", { fontFamily: [Settings.font, "NotoColorEmoji"], fontSize: Settings.nameSize * 4, fill: 0xffffff });
        this.text.scale.x = 0.25;
        this.text.scale.y = 0.25;
        this.text.style.stroke = "black";
        this.text.style.strokeThickness = 8;
        //this.text.zIndex = 100;
        this.textChat = new PIXI.Text("", { fontFamily: "FontAwesome", fontSize: Settings.nameSize, fill: 0xffffff });
        this.chat = null;
        this.plotly = null;
        this.text.anchor.set(0.5, 0.5);
        this.textChat.anchor.set(0.5, 0.5);
        this.text.position.x = 0;
        this.text.position.y = 0;
        this.textChat.position.x = 0;
        this.textChat.position.y = 0;
        /*this.text.parentGroup = this.container.bodyGroup;
        this.textChat.parentGroup = this.container.bodyGroup;*/
        this.container.addChild(this.text);
        this.container.addChild(this.textChat);
    }

    addShip(ship) {
        this.ships.push(ship);
        ship.fleet = this;
    }

    removeShip(ship) {
        this.ships = this.ships.filter(s => s != ship);
    }

    update(groupUpdate, myFleetID) {
        this.caption = groupUpdate.Caption;
        this.ID = groupUpdate.ID;

        if (groupUpdate.CustomData) {
            if (groupUpdate.CustomData.chat) this.chat = groupUpdate.CustomData.chat;
            else this.chat = null;

            if (groupUpdate.CustomData.plotly) this.plotly = groupUpdate.CustomData.plotly;
            else this.plotly = null;
        }

        if (this.plotly && this.ID == myFleetID) {
            if (!this.container.plotly.used) {
                this.container.plotly.used = true;
                this.usingPlotly = true;
                console.log("setting plotly use");
            }
            Plotly.react(this.container.plotly, this.plotly.data, this.plotly.layout, {
                displayModeBar: false,
                staticPlot: true
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

    preRender(time, interpolator, myFleetID) {
        //console.log(`Group: ${this.ID} ${this.caption} ${this.ships.length}`);
        if (this.ships.length > 0 && (this.ID != myFleetID || document.body.classList.contains("spectating"))) {
            if (this.text.visible != Settings.namesEnabled) this.text.visible = Settings.namesEnabled;

            if (Settings.nameSize) {
                if (this.caption) this.text.text = this.caption;
                else this.text.text = "";

                //this.chat = "\uf165 testing";

                if (this.chat) this.textChat.text = this.chat;
                else this.textChat.text = "";

                //this.text.text += " " + this.ships.length;
                let accX = 0,
                    accY = 0,
                    count = 0;

                this.ships.forEach(ship => {
                    const position = interpolator.projectObject(ship.body, time);
                    accX += position.x;
                    accY += position.y;
                    count++;
                });

                const offsetY = 0;
                this.text.position.x = accX / count;
                this.text.position.y = accY / count + offsetY;
                this.textChat.position.x = accX / count;
                this.textChat.position.y = accY / count + offsetY - 200;
            }
        } else {
            this.text.visible = false;
            this.textChat.visible = false;
        }

        //else
        //  this.container.plotly.style.visibility = "hidden";
    }

    destroy() {
        this.container.removeChild(this.text);
        this.container.removeChild(this.textChat);
        if (this.usingPlotly) {
            this.container.plotly.used = false;
            console.log("unsetting plotly use");
        }
    }
}
