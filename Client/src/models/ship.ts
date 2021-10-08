import { RenderedObject } from "../renderedObject";
import { Fleet } from "./fleet";
import { GameContainer } from "../gameContainer";
import { ClientBody, ClientGroup } from "../cache";

export class Ship extends RenderedObject {
    fleet: Fleet | undefined;
    bodyID: number;
    constructor(container: GameContainer, clientBody: ClientBody, group: ClientGroup) {
        super(container, clientBody);
        this.fleet = group?.renderer;
        this.bodyID = clientBody.ID;
        this.fleet?.addShip(this.bodyID, this);
    }

    decodeOrderedModes(mode: number) {
        var modes: string[] = [];

        if ((mode & 16) != 0) modes.push("shield");

        modes.push("default");

        if ((mode & 4) != 0) modes.push("defenseupgrade");

        if ((mode & 8) != 0) modes.push("offenseupgrade");

        if ((mode & 1) != 0) modes.push("boost");

        if ((mode & 2) != 0) modes.push("invulnerable");

        if (this.fleet?.extraModes) modes.push(...this.fleet?.extraModes);

        return modes;
    }

    destroy() {
        //if (this.fleet) this.fleet.deleteShip(this.bodyID);

        super.destroy();
    }

    update() {
        super.update();

        // when a ship is abandoned, the ship lives on
        // but it's disconnected from its group
        if (this.fleet && this.body.Group != this.fleet.ID) {
            this.fleet.deleteShip(this.bodyID);
            this.fleet = undefined;
        }
    }
}

