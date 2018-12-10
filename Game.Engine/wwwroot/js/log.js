import { Settings } from "./settings";

export class Log {
    constructor(canvas, context, settings = {}) {
        this.context = context;
        this.canvas = canvas;
        this.data = [];
        this.lastDisplay = false;
    }

    addEntry(entry) {
        this.data.push({ time: new Date(), entry });
        while (this.data.length > Settings.logLength) this.data.shift();

        this.lastDisplay = performance.now();

        //console.log(this.data);
    }

    draw() {
        const ctx = this.context;
        ctx.save();
        if (this.data && this.lastDisplay && Settings.logLength > 0) {
            const time = performance.now() - this.lastDisplay;

            let alpha = 1.0;
            if (time > 5000) alpha = 1.0 - Math.max(time - 5000, 1000) / 1000.0;
            if (time > 6000) alpha = 0;

            ctx.globalAlpha = alpha;

            ctx.font = `12pt ${Settings.font}`;
            ctx.textAlign = "left";

            const rowHeight = 28;
            const margin = 20;

            for (let i = 0; i < this.data.length; i++) {
                const slot = this.data[i];

                ctx.fillStyle = "gray";
                ctx.fillText(slot.time.toLocaleTimeString(), margin, rowHeight + i * rowHeight);

                ctx.fillStyle = "white";
                ctx.fillText(slot.entry, margin + 100, rowHeight + i * rowHeight);
            }
        }

        ctx.restore();
    }
}
