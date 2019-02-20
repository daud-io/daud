import { Vector2 } from "./Vector2";
import { Dimension2 } from "./Dimension2";

export class Camera {
    distance: number;
    lookat: number[];
    size: Dimension2;
    fieldOfView: number;
    viewport: { left: number; right: number; top: number; bottom: number; width: number; height: number; scale: number[] };
    aspectRatio: number;
    constructor(size:Dimension2, settings = { fieldOfView: Math.PI / 4.0 }) {
        this.distance = 1500.0;
        this.lookat = [0, 0];
        this.size = size;
        this.fieldOfView = settings.fieldOfView || Math.PI / 4.0;
        this.viewport = {
            left: 0,
            right: 0,
            top: 0,
            bottom: 0,
            width: 0,
            height: 0,
            scale: [1.0, 1.0]
        };
        this.updateViewport();
    }

    updateViewport() {
        this.aspectRatio = this.size.width / this.size.height;
        this.viewport.width = this.distance * Math.tan(this.fieldOfView);
        this.viewport.height = this.viewport.width / this.aspectRatio;
        this.viewport.left = this.lookat[0] - this.viewport.width / 2.0;
        this.viewport.top = this.lookat[1] - this.viewport.height / 2.0;
        this.viewport.right = this.viewport.left + this.viewport.width;
        this.viewport.bottom = this.viewport.top + this.viewport.height;
        this.viewport.scale[0] = this.size.width / this.viewport.width;
        this.viewport.scale[1] = this.size.height / this.viewport.height;
    }

    zoomTo(z) {
        this.distance = z;
        this.updateViewport();
    }

    moveTo(position:Vector2) {
        this.lookat[0] = position.x;
        this.lookat[1] = position.y;
        this.updateViewport();
    }

    screenToWorld(pos: Vector2, obj: Vector2 = new Vector2(0, 0)) {
        obj.x = pos.x / this.viewport.scale[0] + this.viewport.left;
        obj.y = pos.y / this.viewport.scale[1] + this.viewport.top;
        return obj;
    }
}
