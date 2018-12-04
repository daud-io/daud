import { Settings } from "./settings";

export var img = new Image();
export var setPattern;
var pattern;
img.src = "img/bg.png";
img.onload = function() {
    setPattern();
};
export class Background {
    constructor(canvas, context, settings) {
        settings = settings || {};

        this.context = context;

        var self = this;

        setPattern = function() {
            pattern = self.context.createPattern(img, "repeat");
        };
        this.canvas = canvas;
    }

    draw(x, y) {
        var ctx = this.context;

        if (Settings.background == "none") {
            ctx.fillStyle = "solid black";
            ctx.fillRect(-100000 + x, -100000 + y, 200000, 200000);
            return;
        }
        

        if (Settings.background == "slow")
            this.parallaxFactor = 200;
        else
            this.parallaxFactor = 100;

        x /= this.parallaxFactor;
        y /= this.parallaxFactor;

        ctx.save();
        ctx.scale(10, 10);
        ctx.fillStyle = pattern;

        ctx.translate(-2 * x, -2 * y);
        ctx.globalAlpha = 1;
        ctx.fillRect(-100000 + x, -100000 + y, 200000, 200000);

        ctx.translate(-x, -y);
        ctx.globalAlpha = 0.4;
        ctx.scale(1.3, 1.3);
        ctx.fillRect(-100000 + x, -100000 + y, 200000, 200000);

        ctx.restore();
    }
}
