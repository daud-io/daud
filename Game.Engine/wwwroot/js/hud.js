import { Settings } from "./settings";

export class HUD {
    constructor(canvas, context, settings = {}) {
        this.context = context;
        this.canvas = canvas;
        this.data = false;
    }

    setData(data) {
        this.data = data;
    }

    draw() {
        const ctx = this.context;
        const margin = 15;

        if (!Settings.hudEnabled) return;

        ctx.save();
        ctx.font = `8pt ${Settings.font}`;
        ctx.fillStyle = "white";
        ctx.fillText(
            `fps: ${window.Game.Stats.framesPerSecond || 0}` +
                ` - players: ${window.Game.Stats.playerCount || 0}` +
                ` - spectators: ${window.Game.Stats.spectatorCount || 0}` +
                ` - ping: ${Math.floor(window.Game.primaryConnection.latency || 0)}`,
            margin,
            this.canvas.height - margin * 2
        );

        ctx.restore();
    }
}
