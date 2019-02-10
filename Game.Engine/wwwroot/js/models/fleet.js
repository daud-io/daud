import { Settings } from "../settings";

export class Fleet {
    constructor(container, cache) {
        this.container = container;
        this.caption = false;
        this.ships = [];
        this.ID = false;

        this.text = new PIXI.Text("", { fontFamily: [Settings.font, "NotoColorEmoji"], fontSize: Settings.nameSize, fill: 0xffffff });
        this.textChat = new PIXI.Text("", { fontFamily: "FontAwesome", fontSize: Settings.nameSize, fill: 0xffffff });
        this.chat = false;
        this.text.anchor.set(0.5, 0.5);
        this.textChat.anchor.set(0.5, 0.5);
        this.text.position.x = 0;
        this.text.position.y = 0;
        this.textChat.position.x = 0;
        this.textChat.position.y = 0;
        this.text.parentGroup = this.container.bodyGroup;
        this.textChat.parentGroup = this.container.bodyGroup;
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

    update(groupUpdate) {
        this.caption = groupUpdate.Caption;
        this.ID = groupUpdate.ID;

        if (groupUpdate.CustomData && groupUpdate.CustomData.chat) this.chat = groupUpdate.CustomData.chat;
        else this.chat = false;
    }

    preRender(time, interpolator, myfleetID) {
        //console.log(`Group: ${this.ID} ${this.caption} ${this.ships.length}`);
        if (this.ships.length > 0 && (this.ID != myfleetID || Settings.showOwnName || document.body.classList.contains("spectating"))) {
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
                    accX += position.X;
                    accY += position.Y;
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
    }

    destroy() {
        this.container.removeChild(this.text);
        this.container.removeChild(this.textChat);
        //console.log("fleet destroyed");
    }
}
