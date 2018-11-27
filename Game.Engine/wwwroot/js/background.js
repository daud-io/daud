export class Background {
    constructor(canvas, context, settings) {
        settings = settings || {};
        this.img = new Image();
        this.img.src = "img/variants/default/bg.png";
        this.context = context;

        this.parallaxFactor = 50;

        var self = this;
        this.img.onload = function() {
            self.pattern = self.context.createPattern(self.img, "repeat");
        };
        this.canvas = canvas;
    }

    draw(x, y) {
        var ctx = this.context;
        x /= this.parallaxFactor;
        y /= this.parallaxFactor;

        ctx.save();
        ctx.scale(10, 10);
        ctx.fillStyle = this.pattern;

        ctx.translate(-2 * x, -2 * y);
        ctx.globalAlpha = 1;
        ctx.fillRect(-100000 + x, -100000 + y, 200000, 200000);

        ctx.translate(-x, -y);
        ctx.globalAlpha = 0.4;
        ctx.fillRect(-100000 + x, -100000 + y, 200000, 200000);

        ctx.restore();
    }
}
