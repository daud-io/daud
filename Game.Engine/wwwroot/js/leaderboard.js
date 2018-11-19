import { Renderer } from "./renderer";

export class Leaderboard {
    constructor(canvas, context, settings = {}) {
        this.context = context;
        this.canvas = canvas;
        this.data = false;
    }

    setData(data) {
        this.data = data;
    }

    draw(relativeTo) {
        const ctx = this.context;
        ctx.save();
        if (this.data && this.data.Entries) {
            ctx.font = "12pt sans-serif";
            ctx.fillStyle = "white";
            ctx.textAlign = "left";

            const width = 200;
            var rowHeight = 28;
            const margin = 20;
            const arrow = Renderer.sprites["arrow"];

            for (let i = 0; i < this.data.Entries.length; i++) {
                const entry = this.data.Entries[i];

                ctx.fillStyle = "white";

                ctx.fillText(entry.Name || "Unknown Fleet", this.canvas.width - width, rowHeight + i * rowHeight);
                ctx.fillText(entry.Score, this.canvas.width - 60, rowHeight + i * rowHeight);

                ctx.fillStyle = entry.Color;

                const x = this.canvas.width - width - rowHeight;
                const y = i * rowHeight + 10;

                ctx.fillRect(x, y, rowHeight, rowHeight);

                if (relativeTo) {
                    const angle = Math.atan2(entry.Position.Y - relativeTo.Y, entry.Position.X - relativeTo.X);

                    ctx.save();
                    ctx.translate(x + rowHeight / 2, y + rowHeight / 2);
                    const w = arrow.image.width;
                    const h = arrow.image.height;
                    ctx.rotate(angle);
                    ctx.scale(arrow.scale, arrow.scale);
                    ctx.drawImage(arrow.image, -w / 2, -h / 2, w, h);
                    ctx.restore();
                }
            }

            if (this.data.Record) {
                ctx.font = "8pt sans-serif";
                ctx.fillStyle = "white";
                ctx.fillText(`record: ${this.data.Record.Name || "Unknown Fleet"} - ${this.data.Record.Score}`, margin, this.canvas.height - margin);
            }
        }

        ctx.restore();
    }
}
