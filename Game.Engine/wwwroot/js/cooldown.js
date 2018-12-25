import { Settings } from "./settings";

const progress = document.getElementById("cooldown");
export class Cooldown {
    constructor(settings = {}) {
        // this.context = context;
        // this.canvas = canvas;
        this.cooldown = false;
    }

    setCooldown(data) {
        this.cooldown = data;
        progress.value = data;
    }
    // draw() {
    //     if (Settings.showCooldown) {
    //         const width = this.canvas.width / 4;
    //         const cd = (this.cooldown / 255) * width;
    //         const ctx = this.context;

    //         ctx.save();

    //         const pos = (this.canvas.width - width) / 2;

    //         var grd = ctx.createLinearGradient(pos, 0, pos + cd, 0);
    //         grd.addColorStop(0, "#008888");
    //         grd.addColorStop(1, "#0066ff");

    //         ctx.fillStyle = "rgba(255, 255, 255, 0.3)";
    //         ctx.fillRect((this.canvas.width - width) / 2, this.canvas.height - 40, width, 7);

    //         ctx.fillStyle = grd;
    //         ctx.fillRect(pos, this.canvas.height - 40, cd, 7);

    //         ctx.restore();
    //     }
    // }
}
