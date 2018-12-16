import { Settings } from "./settings";

export const img = new Image();
export let setPattern;
let pattern;
img.src = require("../img/bg.png");
img.onload = () => {
    setPattern();
};
export class Background {
    constructor(canvas, context, settings = {}) {
        this.context = context;

        const self = this;

        setPattern = () => {
            pattern = self.context.createPattern(img, "repeat");
        };
        this.canvas = canvas;
    }

    draw(x, y) {
        const ctx = this.context;

        if (Settings.background == "none") {
            ctx.fillStyle = "solid black";
            ctx.fillRect(-100000 + x, -100000 + y, 200000, 200000);
            return;
        }

        if (Settings.background == "slow") this.parallaxFactor = 200;
        else this.parallaxFactor = 100;

        x /= this.parallaxFactor;
        y /= this.parallaxFactor;

        ctx.save();
        ctx.scale(10, 10);
        ctx.fillStyle = pattern;

        ctx.translate(-2 * x, -2 * y);
        ctx.globalAlpha = 1;
        ctx.fillRect(-100000 + x, -100000 + y, 200000, 200000);

        if (Settings.background == "fast") {
            ctx.translate(-x, -y);
            ctx.globalAlpha = 0.4;
            ctx.scale(1.3, 1.3);
            ctx.fillRect(-100000 + x, -100000 + y, 200000, 200000);
        }

        ctx.restore();
    }
}
