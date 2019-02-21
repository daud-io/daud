import { Vector2 } from "./Vector2";
import { Dimension2 } from "./Dimension2";
import * as PIXI from "pixi.js";

export class Camera {
    distance: number;
    lookat: number[];
    size: Dimension2;
    fieldOfView: number;
    viewport: { rectangle: PIXI.Rectangle; scale: number[] };
    aspectRatio: number;
    constructor(size: Dimension2, settings = { fieldOfView: Math.PI / 4.0 }) {
        this.distance = 1500.0;
        this.lookat = [0, 0];
        this.size = size;
        this.fieldOfView = settings.fieldOfView || Math.PI / 4.0;
        this.viewport = {
            rectangle: new PIXI.Rectangle(0, 0, 0, 0),
            scale: [1.0, 1.0]
        };
        this.updateViewport();
    }

    updateViewport() {
        this.aspectRatio = this.size.width / this.size.height;
        this.viewport.rectangle = new PIXI.Rectangle(
            this.lookat[0] - (this.distance * Math.tan(this.fieldOfView)) / 2.0,
            this.lookat[1] - (this.distance * Math.tan(this.fieldOfView)) / this.aspectRatio / 2.0,
            this.distance * Math.tan(this.fieldOfView),
            (this.distance * Math.tan(this.fieldOfView)) / this.aspectRatio
        );
        this.viewport.scale[0] = this.size.width / this.viewport.rectangle.width;
        this.viewport.scale[1] = this.size.height / this.viewport.rectangle.height;
    }

    zoomTo(z) {
        this.distance = z;
        this.updateViewport();
    }

    moveTo(position: Vector2) {
        this.lookat[0] = position.x;
        this.lookat[1] = position.y;
        this.updateViewport();
    }

    screenToWorld(pos: Vector2, obj: Vector2 = new Vector2(0, 0)) {
        obj.x = pos.x / this.viewport.scale[0] + this.viewport.rectangle.left;
        obj.y = pos.y / this.viewport.scale[1] + this.viewport.rectangle.top;
        return obj;
    }
}
