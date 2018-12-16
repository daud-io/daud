export class Cooldown {
    constructor(canvas, context, settings = {}) {
        this.context = context;
        this.canvas = canvas;
        this.cooldown = false;
    }

    setCooldown(data) {
        this.cooldown = data;
    }
    draw() {
        const cd = (this.cooldown / 255) * (this.canvas.width / 4);
        const ctx = this.context;

        ctx.save();

        const pos = (this.canvas.width - cd) / 2;

        var grd = ctx.createLinearGradient(pos, 0, pos + cd, 0);
        grd.addColorStop(0, "#008888");
        grd.addColorStop(1, "#0066ff");

        ctx.fillStyle = grd;
        ctx.fillRect(pos, this.canvas.height - 40, cd, 10);

        ctx.restore();
    }
}
