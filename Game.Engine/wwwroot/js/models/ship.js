import { RenderedObject } from "./renderedObject";

export class Ship extends RenderedObject {
    constructor(container) {
        super(container);
        this.fleet = false;
    }

    decodeModes(mode) {
        var modes = [];

        if ((mode & 4) != 0) modes.push("defenseupgrade");

        if ((mode & 8) != 0) modes.push("offenseupgrade");

        modes.push("default");

        if ((mode & 1) != 0) modes.push("boost");

        if ((mode & 2) != 0) modes.push("invulnerable");

        if ((mode & 16) != 0) modes.push("shield");

        return modes;
    }

    static getSelectorImage(spriteName) {
        const spriteDefinition = RenderedObject.getSpriteDefinition(spriteName);

        if (spriteDefinition.selector) return RenderedObject.getTextureImage(spriteDefinition.selector);
        else return false;
    }

    destroy() {
        if (this.fleet) this.fleet.removeShip(this);

        super.destroy();
    }

    update(updateData) {
        super.update(updateData);

        // when a ship is abandoned, the ship lives on
        // but it's disconnected from its group
        if (this.fleet && this.body.Group != this.fleet.ID) {
            this.fleet.removeShip(this);
            this.fleet = false;
        }
    }
}
