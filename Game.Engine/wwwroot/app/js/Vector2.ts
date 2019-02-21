import * as PIXI from "pixi.js";
export class Vector2 extends PIXI.Point {
    constructor(x: number, y: number) {
        super(x, y);
    }
    static Polar(theta: number, r: number): Vector2 {
        return new Vector2(Math.cos(theta) * r, Math.sin(theta) * r);
    }
}
