import { RenderedObject } from "../renderedObject";
import { Fleet } from "./fleet";
import { CustomContainer } from "../CustomContainer";
import { ClientBody } from "../cache";

export class Token extends RenderedObject {
    fleet: Fleet | null;
    bodyID: string;
    constructor(container: CustomContainer, clientBody: ClientBody) {
        super(container, clientBody);
        this.fleet = null;
        this.bodyID = `b-${clientBody.ID}`;
    }

    decodeOrderedModes(mode: number) {
        var modes: string[] = [];

        if (mode == 0) modes.push("default");
        if ((mode & 1) != 0) modes.push("carried");
        if ((mode & 2) != 0) modes.push("expiring");

        return modes;
    }

    destroy() {
        //if (this.fleet) 
            //this.fleet.deleteShip(this.bodyID);

        super.destroy();
    }

    update() {
        super.update();

        // when a ship is abandoned, the ship lives on
        // but it's disconnected from its group
        /*if (this.fleet && this.body.Group != this.fleet.ID) {
            this.fleet.deleteShip(this.bodyID);
            this.fleet = null;
        }*/
    }
}
