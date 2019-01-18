import { Settings } from "./settings";

const minimapSize = 180;
const minimapMarginBottom = 15;
const minimapMarginRight = 15;
const crownImg = document.getElementById("crownImg");

const colors = {
    red: 0xff0000,
    pink: 0xffc0cb,
    orange: 0xffa500,
    yellow: 0xffff00,
    cyan: 0x00ffff,
    green: 0x00ff00,
};

export class Minimap {
    constructor(stage, size) {
        this.ctx = new PIXI.Graphics();
        this.ctx.position.x = size.width - minimapSize - minimapMarginRight;
        this.ctx.position.y = size.height - minimapSize - minimapMarginBottom;
        stage.addChild(this.ctx);
    }
    size(size) {
        this.ctx.position.x = size.width - minimapSize - minimapMarginRight;
        this.ctx.position.y = size.height - minimapSize - minimapMarginBottom;
    }
    checkDisplay() {
        if (Settings.displayMinimap != this.ctx.visible) this.ctx.visible = Settings.displayMinimap;
    }
    update(data, worldSize, fleetID) {
        this.worldSize = worldSize;
        const startIndex = data.Type == "Team" || data.Type == "CTF" ? 2 : 0;
        this.ctx.clear();
        this.ctx
            .lineStyle(1, 0x999999)
            .beginFill(0x000000, 0.5)
            .drawRect(0, 0, minimapSize, minimapSize)
            .endFill()
            .lineStyle(0);
        for (let i = startIndex; i < data.Entries.length; i++) {
            const entry = data.Entries[i];
            var entryIsSelf = entry.FleetID == fleetID;
            this.drawMinimap(entry.Position.X, entry.Position.Y, entry.Color, entryIsSelf, i);
        }
    }
    drawMinimap(x, y, color, self, rank) {
        var minimapX = ((x + this.worldSize) / 2 / this.worldSize) * minimapSize;
        var minimapY = ((y + this.worldSize) / 2 / this.worldSize) * minimapSize;

        if (self) {
			// mark "self" player
			this.ctx
                .beginFill(0xffffff)
                .drawRect(minimapX - 3, minimapY - 3, 6, 6)
                .endFill();
        } else if (rank === 0) {
			// mark the king
			this.ctx
				.beginFill(0xdaa520)
                .drawRect(minimapX - 3, minimapY - 3, 6, 6)
                .endFill();
			/*
			var x = minimapX - 4;
			var y = minimapY - 2;
			const ctx2 = this.ctx;
			this.ctx.beginPath();
			this.ctx.moveTo(0+x, 0+y);
			this.ctx.lineTo(2+x, 2+y);
			this.ctx.lineTo(4+x, 0+y);
			this.ctx.lineTo(6+x, 2+y);
			this.ctx.lineTo(8+x, 0+y);
			this.ctx.lineTo(8+x, 4+y);
			this.ctx.lineTo(0+x, 4+y);
			this.ctx.closePath();
			this.ctx.strokeStyle = "gold";
			this.ctx.stroke();
			this.ctx.fillStyle = "gold";
			this.ctx.fill();
			*/
		} else {
            this.ctx
                .beginFill(colors[color])
                .drawRect(minimapX - 2, minimapY - 2, 4, 4)
                .endFill();
        }
    }
}
