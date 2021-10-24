import { ObjectMode, RenderedObject } from "../renderedObject";
import { Fleet } from "./fleet";
import { GameContainer } from "../gameContainer";
import { ClientBody, ClientGroup } from "../cache";
import { Controls } from "../controls";
import { angleLerp, projectObject } from "../interpolator";

enum ShipModes
{
    shield,
    default,
    defenseupgrade,
    offenseupgrade,
    boost,
    invulnerable
}

export class Ship extends RenderedObject {
    fleet: Fleet | undefined;
    bodyID: number;

    constructor(container: GameContainer, clientBody: ClientBody, group: ClientGroup) {
        super(container, clientBody);
        this.fleet = group?.renderer;
        this.bodyID = clientBody.ID;
        this.fleet?.addShip(this.bodyID, this);
    }

    setupModes(): ObjectMode[]
    {
        return [
            new ObjectMode("shield"),
            new ObjectMode("default"),
            new ObjectMode("defenseupgrade"),
            new ObjectMode("offenseupgrade"),
            new ObjectMode("boost"),
            new ObjectMode("invulnerable")
        ];
    }

    updateMode(mode: number) {
        this.modes[ShipModes.shield].visible = (mode & 16) != 0;
        this.modes[ShipModes.default].visible = true;
        this.modes[ShipModes.defenseupgrade].visible = (mode & 4) != 0;
        this.modes[ShipModes.offenseupgrade].visible = (mode & 8) != 0;
        this.modes[ShipModes.boost].visible = (mode & 1) != 0;
        this.modes[ShipModes.invulnerable].visible = (mode & 2) != 0;
        this.currentMode = mode;
    }

    dispose() {
        //if (this.fleet) this.fleet.deleteShip(this.bodyID);

        super.dispose();
    }

    tick(time: number) {
        if (this.body) {
            projectObject(this.body, time);

            if (this.fleet?.ID == this.container.fleetID 
                && this.container.alive)
            {
                let lookAtX = Controls.mouseX + this.container.cameraPosition.x;
                let lookAtY = Controls.mouseY + this.container.cameraPosition.y;

                let override = Math.min(Controls.pointerSpeed, 100) / 100 / 5;
                let localAngle = Math.atan2(lookAtY-this.body.Position.y, lookAtX-this.body.Position.x);

                this.body.Angle = angleLerp(this.body.Angle, localAngle, override);
            }
    
            for (let key in this.textureLayers) {
                let textureLayer = this.textureLayers[key];
                textureLayer.prerender(time, this.body);
            }
        }

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

