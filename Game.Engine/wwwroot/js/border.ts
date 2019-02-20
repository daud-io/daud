import { Container } from "pixi.js";

export class Border {
    container: Container;
    graphics: PIXI.Graphics;
    worldSize: number;
    constructor(container: Container) {
        this.container = container;

        this.graphics = new PIXI.Graphics();
        this.graphics.parentGroup = (<any>this.container).backgroundGroup;

        this.updateWorldSize(6000);
        this.container.addChild(this.graphics);
    }

    updateWorldSize(size) {
        const edgeWidth = 4000;
        this.graphics.clear();
        this.graphics.beginFill(0xff0000, 0.1);
        this.graphics.drawRect(-size - edgeWidth, -size - edgeWidth, 2 * size + 2 * edgeWidth, edgeWidth);
        this.graphics.drawRect(-size - edgeWidth, -size, edgeWidth, 2 * size);
        this.graphics.drawRect(+size, -size, edgeWidth, 2 * size);
        this.graphics.drawRect(-size - edgeWidth, +size, 2 * size + 2 * edgeWidth, edgeWidth);
        this.graphics.endFill();

        this.graphics.lineStyle(40, 0x0000ff);
        this.graphics.drawRect(-size, -size, size * 2, size * 2);

        this.worldSize = size;
    }

    draw(cache, interpolator, currentTime) {}
}
