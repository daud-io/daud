const minimapSize = 200;

const colors = {
    red: 0xff0000,
    pink: 0xffc0cb,
    orange: 0xffa500,
    yellow: 0xffff00,
    cyan: 0x00ffff,
    green: 0x00ff00
};

export class Minimap {
    constructor(stage, size) {
        this.ctx = new PIXI.Graphics();
        this.ctx.position.x = size.width - minimapSize - 15;
        this.ctx.position.y = size.height - minimapSize - 15;
        stage.addChild(this.ctx);
    }
    size(size) {
        this.ctx.position.x = size.width - minimapSize - 15;
        this.ctx.position.y = size.height - minimapSize - 15;
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
            this.drawMinimap(entry.Position.X, entry.Position.Y, entry.Color, entryIsSelf);
        }
    }
    drawMinimap(x, y, color, self) {
        var minimapX = ((x + this.worldSize) / 2 / this.worldSize) * minimapSize - 2;
		var minimapY = ((y + this.worldSize) / 2 / this.worldSize) * minimapSize - 2;

        if (!self) {
            this.ctx
                .beginFill(colors[color])
                .drawRect(minimapX - 2, minimapY - 2, 4, 4)
                .endFill();
        } else {
            this.ctx
                .beginFill(0xffffff)
                .drawRect(minimapX - 3, minimapY - 3, 6, 6)
                .endFill();
        }
    }
}
