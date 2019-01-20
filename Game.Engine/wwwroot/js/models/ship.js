import { RenderedObject } from "./renderedObject";

export class Ship extends RenderedObject {
    constructor(container) {
        super(container);
        this.fleet = false;
    }

    getMode(mode) {
        switch (mode) {
            case 0:
                return "default";
            case 1:
                return "boost";
            case 2:
                return "weaponupgrade";
            case 3:
                return "invulnerable";
            default:
                return "default";
        }
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
