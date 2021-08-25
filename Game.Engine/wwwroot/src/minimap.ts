import { Vector2 } from "@babylonjs/core";
import { LeaderboardType } from "./connection";
import { GameContainer } from "./gameContainer";
const minimapSize = 180;
const minimapMarginBottom = 15;
const minimapMarginRight = 15;

const colors = {
    red: 0xff0000,
    pink: 0xff00cb,
    orange: 0xffa500,
    yellow: 0xffff00,
    cyan: 0x00ffff,
    blue: 0x2255ff,
    green: 0x00ff00,
};

/*const ctx = new PIXI.Graphics();
ctx.zIndex = 11;

let minimapChanged = false;
window.addEventListener("keydown", (e) => {
    if (e.code == "KeyM" && !minimapChanged && (document.body.classList.contains("alive") || document.body.classList.contains("spectating"))) {
        ctx.visible = !ctx.visible;
        minimapChanged = true;
    }
});

window.addEventListener("keyup", (e) => {
    if (e.code == "KeyM") minimapChanged = false;
});
*/
export class Minimap {
    constructor(container: GameContainer) {
        //stage.addChild(ctx);
    }
    resize(): void {
        //ctx.position = new Vector2(size.width - minimapSize - minimapMarginRight, size.height - minimapSize - minimapMarginBottom);
    }
    update(data: LeaderboardType, worldSize: number, fleetID: number): void {
        /*
        const startIndex = data.Type === "Team" ? 2 : 0;
        const isCTF = data.Type === "CTF";
        ctx.clear();

        if (document.body.classList.contains("alive") || document.body.classList.contains("spectating")) {
            ctx.lineStyle(1, 0x999999).beginFill(0x000000, 0.5).drawRect(0, 0, minimapSize, minimapSize).endFill().lineStyle(0);
            for (let i = startIndex; i < data.Entries.length; i++) {
                const entry = data.Entries[i];
                const entryIsSelf = entry.FleetID == fleetID;
                this.drawMinimap(new Vector2(entry.Position.x, entry.Position.y), entry.Color as keyof typeof colors, entryIsSelf, i, isCTF, worldSize);
            }
        }
        */
    }
    drawMinimap(position: Vector2, color: keyof typeof colors, self: boolean, rank: number, isCTF: boolean, worldSize: number): void {

        /*

        const minimapX = ((position.x + worldSize) / 2 / worldSize) * minimapSize;
        const minimapY = ((position.y + worldSize) / 2 / worldSize) * minimapSize;

        if (self) {
            // mark "self" player
            ctx.beginFill(0xffffff)
                .lineStyle(1, 0xffffff)
                .drawRect(minimapX - 3, minimapY - 3, 6, 6)
                .endFill();
        } else if (rank === 0 && !isCTF) {
            // mark the king
            const x = Math.floor(minimapX - 4);
            const y = Math.floor(minimapY - 2);
            ctx.beginFill(0xdaa520)
                .lineStyle(1, 0xdaa520)
                .moveTo(x, y)
                .lineTo(2 + x, 2 + y)
                .lineTo(4 + x, 0 + y)
                .lineTo(6 + x, 2 + y)
                .lineTo(8 + x, 0 + y)
                .lineTo(8 + x, 4 + y)
                .lineTo(0 + x, 4 + y)
                .closePath()
                .endFill();
        } else if (isCTF && rank < 2) {
            // draw flags in CTF mode
            ctx.lineStyle(2, colors[color])
                .drawRect(minimapX - 4, minimapY - 4, 8, 8)
                .endFill();
        } else {
            ctx.lineStyle(1, colors[color])
                .beginFill(colors[color])
                .drawRect(minimapX - 2, minimapY - 2, 4, 4)
                .endFill();
        }
        */
    }
}
