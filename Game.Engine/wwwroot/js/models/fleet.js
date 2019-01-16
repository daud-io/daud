import { RenderedObject } from "./renderedObject";

export class Fleet {
    constructor(container, cache) {
        this.container = container;
        this.ships = [];
    }

    addShip(ship)
    {
        this.ships.push(ship);
        ship.fleet = this;
    }
    
    update(groupUpdate)
    {
        this.caption = groupUpdate.caption;
    }

    preRender(time, interpolator)
    {


                /*let text = new PIXI.Text(group.Caption, { fontFamily: Settings.font, fontSize: Settings.nameSize, fill: 0xffffff });
                text.anchor.set(0.5, 0.5);
                this.container.addChild(text);
                this.bodies[`p-${group.ID}`] = text;
                if (!Settings.namesEnabled) text.visible = false;*/

        
    }

    destroy()
    {
        console.log("fleet destroyed");
    }
}
